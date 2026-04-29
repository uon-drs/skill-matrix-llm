namespace SkillMatrixLlm.Api.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Constants;
using Data;
using Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Xunit;
using SkillEntity = SkillMatrixLlm.Api.Data.Entities.Skill;
using UserEntity = SkillMatrixLlm.Api.Data.Entities.User;
using UserSkillEntity = SkillMatrixLlm.Api.Data.Entities.UserSkill;

public class SkillsControllerTests(ApiFactory factory) : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly JsonSerializerOptions RequestJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Converters = { new JsonStringEnumConverter() }
    };

    public Task InitializeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.UserSkills.RemoveRange(db.UserSkills);
        db.Skills.RemoveRange(db.Skills);
        db.Users.RemoveRange(db.Users);
        db.SaveChanges();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    // -------------------------------------------------------------------------
    // GET /api/skills
    // -------------------------------------------------------------------------

    [Fact]
    public async Task List_ReturnsAllSkills_WhenNoSearch()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Skills.AddRange(
            new SkillEntity { Name = "C#" },
            new SkillEntity { Name = "TypeScript" }
        );
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.GetAsync("/api/skills");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var skills = await response.Content.ReadFromJsonAsync<List<Skill>>(JsonOptions);
        Assert.NotNull(skills);
        Assert.Equal(2, skills.Count);
    }

    [Fact]
    public async Task List_FiltersByName_CaseInsensitive()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Skills.AddRange(
            new SkillEntity { Name = "C#" },
            new SkillEntity { Name = "TypeScript" }
        );
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.GetAsync("/api/skills?search=typescript");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var skills = await response.Content.ReadFromJsonAsync<List<Skill>>(JsonOptions);
        Assert.NotNull(skills);
        Assert.Single(skills);
        Assert.Equal("TypeScript", skills[0].Name);
    }

    [Fact]
    public async Task List_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        var response = await factory.CreateAnonymousClient().GetAsync("/api/skills");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // POST /api/skills
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Create_ReturnsCreatedSkill_WhenAuthorized()
    {
        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageSkillsRole]);

        var response = await client.PostAsJsonAsync("/api/skills", new { Name = "Kubernetes" }, RequestJsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var skill = await response.Content.ReadFromJsonAsync<Skill>(JsonOptions);
        Assert.NotNull(skill);
        Assert.Equal("Kubernetes", skill.Name);
        Assert.NotEqual(Guid.Empty, skill.Id);
    }

    [Fact]
    public async Task Create_ReturnsConflict_WhenNameAlreadyExists()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Skills.Add(new SkillEntity { Name = "Docker" });
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageSkillsRole]);

        var response = await client.PostAsJsonAsync("/api/skills", new { Name = "Docker" }, RequestJsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsForbidden_WithoutManageSkillsRole()
    {
        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PostAsJsonAsync("/api/skills", new { Name = "Docker" }, RequestJsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // PUT /api/skills/{id}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Rename_ReturnsUpdatedSkill_WhenAuthorized()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var skill = new SkillEntity { Name = "OldName" };
        db.Skills.Add(skill);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageSkillsRole]);

        var response = await client.PutAsJsonAsync($"/api/skills/{skill.Id}", new { Name = "NewName" }, RequestJsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<Skill>(JsonOptions);
        Assert.NotNull(updated);
        Assert.Equal("NewName", updated.Name);
    }

    [Fact]
    public async Task Rename_ReturnsNotFound_WhenSkillDoesNotExist()
    {
        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageSkillsRole]);

        var response = await client.PutAsJsonAsync($"/api/skills/{Guid.NewGuid()}", new { Name = "NewName" }, RequestJsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Rename_ReturnsConflict_WhenNewNameAlreadyExists()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Skills.AddRange(
            new SkillEntity { Name = "Go" },
            new SkillEntity { Name = "Rust" }
        );
        await db.SaveChangesAsync();
        var goSkill = db.Skills.First(s => s.Name == "Go");

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageSkillsRole]);

        var response = await client.PutAsJsonAsync($"/api/skills/{goSkill.Id}", new { Name = "Rust" }, RequestJsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Rename_ReturnsForbidden_WithoutManageSkillsRole()
    {
        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PutAsJsonAsync($"/api/skills/{Guid.NewGuid()}", new { Name = "NewName" }, RequestJsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // DELETE /api/skills/{id}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenSkillIsUnassigned()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var skill = new SkillEntity { Name = "Fortran" };
        db.Skills.Add(skill);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageSkillsRole]);

        var response = await client.DeleteAsync($"/api/skills/{skill.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        db.ChangeTracker.Clear();
        Assert.Null(db.Skills.Find(skill.Id));
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenSkillDoesNotExist()
    {
        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageSkillsRole]);

        var response = await client.DeleteAsync($"/api/skills/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsConflict_WhenSkillIsAssignedToUser()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new UserEntity { KeycloakId = "kc-skill-test", DisplayName = "Test User", Email = "t@example.com" };
        var skill = new SkillEntity { Name = "Python" };
        db.Users.Add(user);
        db.Skills.Add(skill);
        await db.SaveChangesAsync();
        db.UserSkills.Add(new UserSkillEntity { UserId = user.Id, SkillId = skill.Id, Level = Enums.Level.Basic });
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageSkillsRole]);

        var response = await client.DeleteAsync($"/api/skills/{skill.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsForbidden_WithoutManageSkillsRole()
    {
        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.DeleteAsync($"/api/skills/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
