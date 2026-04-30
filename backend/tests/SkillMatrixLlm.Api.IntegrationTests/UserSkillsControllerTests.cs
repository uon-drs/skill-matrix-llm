namespace SkillMatrixLlm.Api.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Constants;
using Data;
using Enums;
using Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Models.User;
using Xunit;
using SkillEntity = SkillMatrixLlm.Api.Data.Entities.Skill;
using UserEntity = SkillMatrixLlm.Api.Data.Entities.User;
using UserSkillEntity = SkillMatrixLlm.Api.Data.Entities.UserSkill;

public class UserSkillsControllerTests(ApiFactory factory) : IClassFixture<ApiFactory>, IAsyncLifetime
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
    // POST /api/users/{userId}/skills
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AddSkill_ReturnsCreated_WhenUserAddsSelf()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new UserEntity { KeycloakId = TestClaims.Sub.Value, DisplayName = "Test", Email = "t@example.com" };
        var skill = new SkillEntity { Name = "C#" };
        db.Users.Add(user);
        db.Skills.Add(skill);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PostAsJsonAsync(
            $"/api/users/{user.Id}/skills",
            new { SkillId = skill.Id, Level = Level.Basic },
            RequestJsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<UserSkillDto>(JsonOptions);
        Assert.NotNull(dto);
        Assert.Equal(skill.Id, dto.SkillId);
        Assert.Equal("C#", dto.SkillName);
        Assert.Equal(Level.Basic, dto.Level);
    }

    [Fact]
    public async Task AddSkill_ReturnsCreated_WhenAdmin()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var owner = new UserEntity { KeycloakId = "other-kc-id", DisplayName = "Owner", Email = "owner@example.com" };
        var skill = new SkillEntity { Name = "Docker" };
        db.Users.Add(owner);
        db.Skills.Add(skill);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.UpdateUsersRole]);

        var response = await client.PostAsJsonAsync(
            $"/api/users/{owner.Id}/skills",
            new { SkillId = skill.Id, Level = Level.Intermediate },
            RequestJsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task AddSkill_ReturnsForbidden_WhenNotOwnerOrAdmin()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var caller = new UserEntity { KeycloakId = TestClaims.Sub.Value, DisplayName = "Caller", Email = "caller@example.com" };
        var owner = new UserEntity { KeycloakId = "other-kc-id", DisplayName = "Owner", Email = "owner@example.com" };
        var skill = new SkillEntity { Name = "Python" };
        db.Users.AddRange(caller, owner);
        db.Skills.Add(skill);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PostAsJsonAsync(
            $"/api/users/{owner.Id}/skills",
            new { SkillId = skill.Id, Level = Level.Basic },
            RequestJsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AddSkill_ReturnsConflict_WhenSkillAlreadyAssigned()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new UserEntity { KeycloakId = TestClaims.Sub.Value, DisplayName = "Test", Email = "t@example.com" };
        var skill = new SkillEntity { Name = "Go" };
        db.Users.Add(user);
        db.Skills.Add(skill);
        await db.SaveChangesAsync();
        db.UserSkills.Add(new UserSkillEntity { UserId = user.Id, SkillId = skill.Id, Level = Level.Basic });
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PostAsJsonAsync(
            $"/api/users/{user.Id}/skills",
            new { SkillId = skill.Id, Level = Level.Pro },
            RequestJsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AddSkill_ReturnsNotFound_WhenSkillDoesNotExist()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new UserEntity { KeycloakId = TestClaims.Sub.Value, DisplayName = "Test", Email = "t@example.com" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PostAsJsonAsync(
            $"/api/users/{user.Id}/skills",
            new { SkillId = Guid.NewGuid(), Level = Level.Basic },
            RequestJsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // PUT /api/users/{userId}/skills/{skillId}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task UpdateSkillLevel_ReturnsOk_WhenUserUpdatesSelf()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new UserEntity { KeycloakId = TestClaims.Sub.Value, DisplayName = "Test", Email = "t@example.com" };
        var skill = new SkillEntity { Name = "Rust" };
        db.Users.Add(user);
        db.Skills.Add(skill);
        await db.SaveChangesAsync();
        db.UserSkills.Add(new UserSkillEntity { UserId = user.Id, SkillId = skill.Id, Level = Level.Basic });
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PutAsJsonAsync(
            $"/api/users/{user.Id}/skills/{skill.Id}",
            new { Level = Level.Pro },
            RequestJsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var dto = await response.Content.ReadFromJsonAsync<UserSkillDto>(JsonOptions);
        Assert.NotNull(dto);
        Assert.Equal(Level.Pro, dto.Level);
    }

    [Fact]
    public async Task UpdateSkillLevel_ReturnsNotFound_WhenSkillNotAssigned()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new UserEntity { KeycloakId = TestClaims.Sub.Value, DisplayName = "Test", Email = "t@example.com" };
        var skill = new SkillEntity { Name = "Elixir" };
        db.Users.Add(user);
        db.Skills.Add(skill);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PutAsJsonAsync(
            $"/api/users/{user.Id}/skills/{skill.Id}",
            new { Level = Level.Intermediate },
            RequestJsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSkillLevel_ReturnsForbidden_WhenNotOwnerOrAdmin()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var caller = new UserEntity { KeycloakId = TestClaims.Sub.Value, DisplayName = "Caller", Email = "caller@example.com" };
        var owner = new UserEntity { KeycloakId = "other-kc-id", DisplayName = "Owner", Email = "owner@example.com" };
        var skill = new SkillEntity { Name = "Kotlin" };
        db.Users.AddRange(caller, owner);
        db.Skills.Add(skill);
        await db.SaveChangesAsync();
        db.UserSkills.Add(new UserSkillEntity { UserId = owner.Id, SkillId = skill.Id, Level = Level.Basic });
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PutAsJsonAsync(
            $"/api/users/{owner.Id}/skills/{skill.Id}",
            new { Level = Level.Pro },
            RequestJsonOptions);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // DELETE /api/users/{userId}/skills/{skillId}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RemoveSkill_ReturnsNoContent_WhenUserRemovesSelf()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new UserEntity { KeycloakId = TestClaims.Sub.Value, DisplayName = "Test", Email = "t@example.com" };
        var skill = new SkillEntity { Name = "Swift" };
        db.Users.Add(user);
        db.Skills.Add(skill);
        await db.SaveChangesAsync();
        db.UserSkills.Add(new UserSkillEntity { UserId = user.Id, SkillId = skill.Id, Level = Level.Basic });
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.DeleteAsync($"/api/users/{user.Id}/skills/{skill.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        db.ChangeTracker.Clear();
        Assert.False(db.UserSkills.Any(us => us.UserId == user.Id && us.SkillId == skill.Id));
    }

    [Fact]
    public async Task RemoveSkill_ReturnsNoContent_WhenAdmin()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var owner = new UserEntity { KeycloakId = "other-kc-id", DisplayName = "Owner", Email = "owner@example.com" };
        var skill = new SkillEntity { Name = "Scala" };
        db.Users.Add(owner);
        db.Skills.Add(skill);
        await db.SaveChangesAsync();
        db.UserSkills.Add(new UserSkillEntity { UserId = owner.Id, SkillId = skill.Id, Level = Level.Intermediate });
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.UpdateUsersRole]);

        var response = await client.DeleteAsync($"/api/users/{owner.Id}/skills/{skill.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task RemoveSkill_ReturnsNotFound_WhenSkillNotAssigned()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new UserEntity { KeycloakId = TestClaims.Sub.Value, DisplayName = "Test", Email = "t@example.com" };
        var skill = new SkillEntity { Name = "Haskell" };
        db.Users.Add(user);
        db.Skills.Add(skill);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.DeleteAsync($"/api/users/{user.Id}/skills/{skill.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task RemoveSkill_ReturnsForbidden_WhenNotOwnerOrAdmin()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var caller = new UserEntity { KeycloakId = TestClaims.Sub.Value, DisplayName = "Caller", Email = "caller@example.com" };
        var owner = new UserEntity { KeycloakId = "other-kc-id", DisplayName = "Owner", Email = "owner@example.com" };
        var skill = new SkillEntity { Name = "Lua" };
        db.Users.AddRange(caller, owner);
        db.Skills.Add(skill);
        await db.SaveChangesAsync();
        db.UserSkills.Add(new UserSkillEntity { UserId = owner.Id, SkillId = skill.Id, Level = Level.Basic });
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.DeleteAsync($"/api/users/{owner.Id}/skills/{skill.Id}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // GET /api/users/{userId}/skills/llm-prompt
    // -------------------------------------------------------------------------

    [Fact]
    public async Task GetSkillsForLlmPrompt_ReturnsBulletList_WhenSkillsExist()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new UserEntity { KeycloakId = TestClaims.Sub.Value, DisplayName = "Test", Email = "t@example.com" };
        var skillA = new SkillEntity { Name = "Python" };
        var skillB = new SkillEntity { Name = "TypeScript" };
        db.Users.Add(user);
        db.Skills.AddRange(skillA, skillB);
        await db.SaveChangesAsync();
        db.UserSkills.AddRange(
            new UserSkillEntity { UserId = user.Id, SkillId = skillA.Id, Level = Level.Pro },
            new UserSkillEntity { UserId = user.Id, SkillId = skillB.Id, Level = Level.Intermediate }
        );
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.GetAsync($"/api/users/{user.Id}/skills/llm-prompt");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("- Python (Pro)", body);
        Assert.Contains("- TypeScript (Intermediate)", body);
    }

    [Fact]
    public async Task GetSkillsForLlmPrompt_ReturnsEmptyString_WhenNoSkills()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = new UserEntity { KeycloakId = TestClaims.Sub.Value, DisplayName = "Test", Email = "t@example.com" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.GetAsync($"/api/users/{user.Id}/skills/llm-prompt");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(string.Empty, body.Trim('"'));
    }

    [Fact]
    public async Task GetSkillsForLlmPrompt_ReturnsForbidden_WhenNotOwnerOrAdmin()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var caller = new UserEntity { KeycloakId = TestClaims.Sub.Value, DisplayName = "Caller", Email = "caller@example.com" };
        var owner = new UserEntity { KeycloakId = "other-kc-id", DisplayName = "Owner", Email = "owner@example.com" };
        db.Users.AddRange(caller, owner);
        await db.SaveChangesAsync();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.GetAsync($"/api/users/{owner.Id}/skills/llm-prompt");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
