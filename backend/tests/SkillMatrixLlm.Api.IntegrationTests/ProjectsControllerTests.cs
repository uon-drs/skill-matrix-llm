namespace SkillMatrixLlm.Api.Tests;

using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Constants;
using Data;
using Enums;
using Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Models;
using Models.Projects;
using Xunit;
using ProjectEntity = SkillMatrixLlm.Api.Data.Entities.Project;
using TeamEntity = SkillMatrixLlm.Api.Data.Entities.Team;
using TeamMembershipEntity = SkillMatrixLlm.Api.Data.Entities.TeamMembership;
using UserEntity = SkillMatrixLlm.Api.Data.Entities.User;

public class ProjectsControllerTests(ApiFactory factory) : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private static readonly JsonSerializerOptions RequestJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() }
    };

    public Task InitializeAsync()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Recommendations.RemoveRange(db.Recommendations);
        db.TeamMemberships.RemoveRange(db.TeamMemberships);
        db.Teams.RemoveRange(db.Teams);
        db.Projects.RemoveRange(db.Projects);
        db.UserSkills.RemoveRange(db.UserSkills);
        db.Users.RemoveRange(db.Users);
        db.SaveChanges();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private static UserEntity SeedUser(AppDbContext db, string keycloakId = "test-keycloak-id")
    {
        var user = new UserEntity { KeycloakId = keycloakId, DisplayName = "PM", Email = "pm@example.com" };
        db.Users.Add(user);
        db.SaveChanges();
        return user;
    }

    private static ProjectEntity SeedProject(AppDbContext db, UserEntity user, ProjectStatus status = ProjectStatus.Draft)
    {
        var project = new ProjectEntity
        {
            Title = "Test Project",
            Description = "A description",
            DesiredTeamSize = 3,
            Timeline = "3 months",
            Status = status,
            CreatedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow,
        };
        db.Projects.Add(project);
        db.SaveChanges();
        return project;
    }

    private static void SeedMembership(AppDbContext db, ProjectEntity project, UserEntity member)
    {
        var team = new TeamEntity
        {
            ProjectId = project.Id,
            Source = ProjectSource.ManuallyAssembled,
            Status = TeamStatus.Proposed,
            CreatedAt = DateTime.UtcNow,
        };
        db.Teams.Add(team);
        db.SaveChanges();

        db.TeamMemberships.Add(new TeamMembershipEntity
        {
            TeamId = team.Id,
            UserId = member.Id,
            ProjectRole = "Developer",
            MembershipStatus = MembershipStatus.Invited,
        });
        db.SaveChanges();
    }

    // -------------------------------------------------------------------------
    // GET /api/projects
    // -------------------------------------------------------------------------

    [Fact]
    public async Task List_ReturnsOwnProjects_WhenNoFilter()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = SeedUser(db);
        SeedProject(db, user, ProjectStatus.Draft);
        SeedProject(db, user, ProjectStatus.Open);

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<Project>>(JsonOptions);
        Assert.NotNull(list);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task List_FiltersByStatus()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = SeedUser(db);
        SeedProject(db, user, ProjectStatus.Draft);
        SeedProject(db, user, ProjectStatus.Open);

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.GetAsync("/api/projects?status=Draft");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<Project>>(JsonOptions);
        Assert.NotNull(list);
        Assert.Single(list);
        Assert.Equal(ProjectStatus.Draft, list[0].Status);
    }

    [Fact]
    public async Task List_ReturnsEmpty_WhenCallerHasNoProjects()
    {
        var response = await factory.CreateAuthenticatedClient([TestClaims.Sub]).GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<Project>>(JsonOptions);
        Assert.NotNull(list);
        Assert.Empty(list);
    }

    [Fact]
    public async Task List_IncludesMemberProject_WhenCallerIsTeamMember()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pm = SeedUser(db, "pm-keycloak-id");
        var member = SeedUser(db, "member-keycloak-id");
        var project = SeedProject(db, pm);
        SeedMembership(db, project, member);

        var client = factory.CreateAuthenticatedClient([new Claim("sub", "member-keycloak-id")]);

        var response = await client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<Project>>(JsonOptions);
        Assert.NotNull(list);
        Assert.Single(list);
        Assert.Equal(project.Id, list[0].Id);
    }

    [Fact]
    public async Task List_ExcludesProject_WhenCallerIsNotCreatorOrMember()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pm = SeedUser(db, "pm-keycloak-id");
        SeedProject(db, pm);
        SeedUser(db, "other-keycloak-id");

        var client = factory.CreateAuthenticatedClient([new Claim("sub", "other-keycloak-id")]);

        var response = await client.GetAsync("/api/projects");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var list = await response.Content.ReadFromJsonAsync<List<Project>>(JsonOptions);
        Assert.NotNull(list);
        Assert.Empty(list);
    }

    // -------------------------------------------------------------------------
    // GET /api/projects/{id}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Get_ReturnsProjectDetail_WhenCallerIsCreator()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = SeedUser(db);
        var project = SeedProject(db, user);

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.GetAsync($"/api/projects/{project.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await response.Content.ReadFromJsonAsync<ProjectDetailDto>(JsonOptions);
        Assert.NotNull(detail);
        Assert.Equal(project.Id, detail.Id);
        Assert.Empty(detail.Teams);
        Assert.Empty(detail.Recommendations);
    }

    [Fact]
    public async Task Get_ReturnsProjectDetail_WhenCallerIsTeamMember()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pm = SeedUser(db, "pm-keycloak-id");
        var member = SeedUser(db, "member-keycloak-id");
        var project = SeedProject(db, pm);
        SeedMembership(db, project, member);

        var client = factory.CreateAuthenticatedClient([new Claim("sub", "member-keycloak-id")]);

        var response = await client.GetAsync($"/api/projects/{project.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var detail = await response.Content.ReadFromJsonAsync<ProjectDetailDto>(JsonOptions);
        Assert.NotNull(detail);
        Assert.Equal(project.Id, detail.Id);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenCallerIsNotCreatorOrMember()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pm = SeedUser(db, "pm-keycloak-id");
        var project = SeedProject(db, pm);
        SeedUser(db, "other-keycloak-id");

        var client = factory.CreateAuthenticatedClient([new Claim("sub", "other-keycloak-id")]);

        var response = await client.GetAsync($"/api/projects/{project.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenProjectDoesNotExist()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedUser(db);

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.GetAsync($"/api/projects/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // POST /api/projects
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Create_ReturnsCreatedProject_InDraftStatus()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedUser(db);

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

        var response = await client.PostAsJsonAsync("/api/projects", new
        {
            Title = "My Project",
            Description = "Desc",
            DesiredTeamSize = 4,
            Timeline = "6 weeks"
        }, RequestJsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var project = await response.Content.ReadFromJsonAsync<Project>(JsonOptions);
        Assert.NotNull(project);
        Assert.Equal("My Project", project.Title);
        Assert.Equal(ProjectStatus.Draft, project.Status);
        Assert.NotEqual(Guid.Empty, project.Id);
    }

    [Fact]
    public async Task Create_ReturnsNotFound_WhenCallerHasNoAppRecord()
    {
        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

        var response = await client.PostAsJsonAsync("/api/projects", new
        {
            Title = "X",
            Description = "Y",
            DesiredTeamSize = 1,
            Timeline = "1 week"
        }, RequestJsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // PUT /api/projects/{id}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Update_ReturnsUpdatedProject_WhenDraft()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = SeedUser(db);
        var project = SeedProject(db, user, ProjectStatus.Draft);

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

        var response = await client.PutAsJsonAsync($"/api/projects/{project.Id}", new
        {
            Title = "Updated Title",
            Description = "Updated Desc",
            DesiredTeamSize = 5,
            Timeline = "4 months"
        }, RequestJsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<Project>(JsonOptions);
        Assert.NotNull(updated);
        Assert.Equal("Updated Title", updated.Title);
        Assert.Equal(5, updated.DesiredTeamSize);
    }

    [Fact]
    public async Task Update_ReturnsConflict_WhenProjectIsClosed()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = SeedUser(db);
        var project = SeedProject(db, user, ProjectStatus.Closed);

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

        var response = await client.PutAsJsonAsync($"/api/projects/{project.Id}", new
        {
            Title = "X",
            Description = "Y",
            DesiredTeamSize = 1,
            Timeline = "1 week"
        }, RequestJsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // PUT /api/projects/{id}/status
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(ProjectStatus.Draft, ProjectStatus.Open)]
    [InlineData(ProjectStatus.Open, ProjectStatus.TeamConfirmed)]
    [InlineData(ProjectStatus.Open, ProjectStatus.Closed)]
    [InlineData(ProjectStatus.TeamConfirmed, ProjectStatus.Closed)]
    public async Task TransitionStatus_ReturnsOk_ForValidTransitions(ProjectStatus from, ProjectStatus to)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = SeedUser(db);
        var project = SeedProject(db, user, from);

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

        var response = await client.PutAsJsonAsync(
            $"/api/projects/{project.Id}/status",
            new { Status = to },
            RequestJsonOptions);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<Project>(JsonOptions);
        Assert.NotNull(updated);
        Assert.Equal(to, updated.Status);
    }

    [Theory]
    [InlineData(ProjectStatus.Draft, ProjectStatus.TeamConfirmed)]
    [InlineData(ProjectStatus.Draft, ProjectStatus.Closed)]
    [InlineData(ProjectStatus.Closed, ProjectStatus.Open)]
    public async Task TransitionStatus_ReturnsConflict_ForInvalidTransitions(ProjectStatus from, ProjectStatus to)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = SeedUser(db);
        var project = SeedProject(db, user, from);

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

        var response = await client.PutAsJsonAsync(
            $"/api/projects/{project.Id}/status",
            new { Status = to },
            RequestJsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // DELETE /api/projects/{id}
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Close_ReturnsNoContent_WhenOpen()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = SeedUser(db);
        var project = SeedProject(db, user, ProjectStatus.Open);

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

        var response = await client.DeleteAsync($"/api/projects/{project.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        db.ChangeTracker.Clear();
        Assert.Equal(ProjectStatus.Closed, db.Projects.Find(project.Id)!.Status);
    }

    [Fact]
    public async Task Close_ReturnsConflict_WhenAlreadyClosed()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var user = SeedUser(db);
        var project = SeedProject(db, user, ProjectStatus.Closed);

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

        var response = await client.DeleteAsync($"/api/projects/{project.Id}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Close_ReturnsNotFound_WhenProjectDoesNotExist()
    {
        var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

        var response = await client.DeleteAsync($"/api/projects/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
