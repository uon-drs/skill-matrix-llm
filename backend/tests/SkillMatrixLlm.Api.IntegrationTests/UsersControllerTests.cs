namespace SkillMatrixLlm.Api.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Constants;
using Data;
using Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Models.User;
using SkillMatrixLlm.Api.Enums;
using Xunit;
using UserEntity = SkillMatrixLlm.Api.Data.Entities.User;

public class UsersControllerTests(ApiFactory factory) : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public Task InitializeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.UserSkills.RemoveRange(db.UserSkills);
        db.Users.RemoveRange(db.Users);
        db.SaveChanges();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // -------------------------------------------------------------------------
    // POST /api/users/login
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Login_CreatesNewUser_WhenFirstLogin()
    {
        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.Name, TestClaims.Email]);

        var response = await client.PostAsync("/api/users/login", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = db.Users.FirstOrDefault(u => u.KeycloakId == TestClaims.Sub.Value);
        Assert.NotNull(user);
        Assert.Equal("test-user", user.DisplayName);
        Assert.Equal("test@example.com", user.Email);
    }

    [Fact]
    public async Task Login_UpdatesDisplayNameAndEmail_WhenClaimsChanged()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.Add(new UserEntity
        {
            KeycloakId = TestClaims.Sub.Value,
            DisplayName = "Old Name",
            Email = "old@example.com"
        });
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.Name, TestClaims.Email]);
        var response = await client.PostAsync("/api/users/login", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        db.ChangeTracker.Clear();
        var user = db.Users.First(u => u.KeycloakId == TestClaims.Sub.Value);
        Assert.Equal("test-user", user.DisplayName);
        Assert.Equal("test@example.com", user.Email);
    }

    [Fact]
    public async Task Login_ReturnsUserProfile_WithSkills()
    {
        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.Name, TestClaims.Email]);

        var response = await client.PostAsync("/api/users/login", null);

        var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>(JsonOptions);
        Assert.NotNull(profile);
        Assert.Equal("test-user", profile.DisplayName);
        Assert.Empty(profile.Skills);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WithoutToken()
    {
        var response = await factory.CreateAnonymousClient().PostAsync("/api/users/login", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // GET /api/users/me
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetMe_ReturnsCurrentUserProfile_WhenAuthenticated()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.Add(new UserEntity
        {
            KeycloakId = TestClaims.Sub.Value,
            DisplayName = "test-user",
            Email = "test@example.com"
        });
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.Name]);

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var profile = await response.Content.ReadFromJsonAsync<UserProfileDto>(JsonOptions);
        Assert.NotNull(profile);
        Assert.Equal("test-user", profile.DisplayName);
    }

    [Fact]
    public async Task GetMe_ReturnsNotFound_WhenUserRecordDoesNotExist()
    {
        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.Name]);

        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetMe_ReturnsUnauthorized_WithoutToken()
    {
        var response = await factory.CreateAnonymousClient().GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // GET /api/users (DB-backed list with filters)
    // -------------------------------------------------------------------------

    [Fact]
    public async Task ListUsers_ReturnsAll_WhenNoFilters()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.AddRange(
            new UserEntity { KeycloakId = "kc-1", DisplayName = "Alice", Email = "alice@example.com" },
            new UserEntity { KeycloakId = "kc-2", DisplayName = "Bob", Email = "bob@example.com" }
        );
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ViewUsersRole]);

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = await response.Content.ReadFromJsonAsync<List<UserSummaryDto>>(JsonOptions);
        Assert.NotNull(users);
        Assert.Equal(2, users.Count);
    }

    [Fact]
    public async Task ListUsers_FiltersByName_CaseInsensitive()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.AddRange(
            new UserEntity { KeycloakId = "kc-1", DisplayName = "Alice Smith", Email = "alice@example.com" },
            new UserEntity { KeycloakId = "kc-2", DisplayName = "Bob Jones", Email = "bob@example.com" }
        );
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ViewUsersRole]);

        var response = await client.GetAsync("/api/users?name=ALICE");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = await response.Content.ReadFromJsonAsync<List<UserSummaryDto>>(JsonOptions);
        Assert.NotNull(users);
        Assert.Single(users);
        Assert.Equal("Alice Smith", users[0].DisplayName);
    }

    [Fact]
    public async Task ListUsers_FiltersByRole()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Users.AddRange(
            new UserEntity { KeycloakId = "kc-1", DisplayName = "Alice", Email = "alice@example.com", Role = Role.Admin },
            new UserEntity { KeycloakId = "kc-2", DisplayName = "Bob", Email = "bob@example.com", Role = Role.User }
        );
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ViewUsersRole]);

        var response = await client.GetAsync("/api/users?role=Admin");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var users = await response.Content.ReadFromJsonAsync<List<UserSummaryDto>>(JsonOptions);
        Assert.NotNull(users);
        Assert.Single(users);
        Assert.Equal("Alice", users[0].DisplayName);
    }

    [Fact]
    public async Task ListUsers_ReturnsForbidden_WithoutViewUsersRole()
    {
        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.GetAsync("/api/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // PUT /api/users/{id}/role
    // -------------------------------------------------------------------------

    [Fact]
    public async Task SetRole_UpdatesRole_WhenAdmin()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new UserEntity { KeycloakId = "kc-target", DisplayName = "Target", Email = "target@example.com", Role = Role.User };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.UpdateUsersRole]);

        var response = await client.PutAsJsonAsync($"/api/users/{user.Id}/role", new { Role = "Admin" });

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        db.ChangeTracker.Clear();
        var updated = db.Users.Find(user.Id);
        Assert.Equal(Role.Admin, updated!.Role);
    }

    [Fact]
    public async Task SetRole_ReturnsForbidden_WhenNotAdmin()
    {
        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}/role", new { Role = "Admin" });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task SetRole_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.UpdateUsersRole]);

        var response = await client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}/role", new { Role = "Admin" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
