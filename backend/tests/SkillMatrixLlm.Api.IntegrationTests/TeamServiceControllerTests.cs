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
using Models;
using Models.Teams;
using Xunit;
using ProjectEntity = SkillMatrixLlm.Api.Data.Entities.Project;
using TeamEntity = SkillMatrixLlm.Api.Data.Entities.Team;
using TeamMembershipEntity = SkillMatrixLlm.Api.Data.Entities.TeamMembership;
using UserEntity = SkillMatrixLlm.Api.Data.Entities.User;

public class TeamServiceControllerTests(ApiFactory factory) : IClassFixture<ApiFactory>, IAsyncLifetime
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

  private static ProjectEntity SeedProject(AppDbContext db, UserEntity owner, ProjectStatus status = ProjectStatus.Open)
  {
    var project = new ProjectEntity
    {
      Title = "Test Project",
      Description = "Desc",
      DesiredTeamSize = 3,
      Timeline = "3 months",
      Status = status,
      CreatedByUserId = owner.Id,
      CreatedAt = DateTime.UtcNow,
    };
    db.Projects.Add(project);
    db.SaveChanges();
    return project;
  }

  private static TeamEntity SeedTeam(AppDbContext db, ProjectEntity project, TeamStatus status = TeamStatus.Proposed)
  {
    var team = new TeamEntity
    {
      ProjectId = project.Id,
      Source = ProjectSource.ManuallyAssembled,
      Status = status,
      CreatedAt = DateTime.UtcNow,
    };
    db.Teams.Add(team);
    db.SaveChanges();
    return team;
  }

  // -------------------------------------------------------------------------
  // POST /api/projects/{projectId}/teams
  // -------------------------------------------------------------------------

  [Fact]
  public async Task CreateTeam_ReturnsCreated_WhenProjectExists()
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var user = SeedUser(db);
    var project = SeedProject(db, user);

    var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

    var response = await client.PostAsJsonAsync(
        $"/api/projects/{project.Id}/teams",
        new { Source = ProjectSource.ManuallyAssembled },
        RequestJsonOptions);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    var team = await response.Content.ReadFromJsonAsync<TeamDto>(JsonOptions);
    Assert.NotNull(team);
    Assert.Equal(TeamStatus.Proposed, team.Status);
    Assert.Equal(ProjectSource.ManuallyAssembled, team.Source);
    Assert.Empty(team.Members);
  }

  [Fact]
  public async Task CreateTeam_ReturnsNotFound_WhenProjectDoesNotExist()
  {
    var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

    var response = await client.PostAsJsonAsync(
        $"/api/projects/{Guid.NewGuid()}/teams",
        new { Source = ProjectSource.LlmGenerated },
        RequestJsonOptions);

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  // -------------------------------------------------------------------------
  // POST /api/projects/{projectId}/teams/{teamId}/members
  // -------------------------------------------------------------------------

  [Fact]
  public async Task AddMember_ReturnsCreated_WhenValid()
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var pm = SeedUser(db);
    var member = new UserEntity { KeycloakId = "member-kc", DisplayName = "Member", Email = "m@example.com" };
    db.Users.Add(member);
    db.SaveChanges();
    var project = SeedProject(db, pm);
    var team = SeedTeam(db, project);

    var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

    var response = await client.PostAsJsonAsync(
        $"/api/projects/{project.Id}/teams/{team.Id}/members",
        new { UserId = member.Id, ProjectRole = "Developer" },
        RequestJsonOptions);

    Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    var membership = await response.Content.ReadFromJsonAsync<TeamMembership>(JsonOptions);
    Assert.NotNull(membership);
    Assert.Equal("Developer", membership.ProjectRole);
    Assert.Equal(MembershipStatus.Invited, membership.MembershipStatus);
  }

  [Fact]
  public async Task AddMember_ReturnsConflict_WhenAlreadyMember()
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var pm = SeedUser(db);
    var member = new UserEntity { KeycloakId = "member-kc", DisplayName = "Member", Email = "m@example.com" };
    db.Users.Add(member);
    db.SaveChanges();
    var project = SeedProject(db, pm);
    var team = SeedTeam(db, project);
    db.TeamMemberships.Add(new TeamMembershipEntity { TeamId = team.Id, UserId = member.Id, ProjectRole = "Dev", MembershipStatus = MembershipStatus.Invited });
    db.SaveChanges();

    var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

    var response = await client.PostAsJsonAsync(
        $"/api/projects/{project.Id}/teams/{team.Id}/members",
        new { UserId = member.Id, ProjectRole = "Developer" },
        RequestJsonOptions);

    Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
  }

  // -------------------------------------------------------------------------
  // DELETE /api/projects/{projectId}/teams/{teamId}/members/{userId}
  // -------------------------------------------------------------------------

  [Fact]
  public async Task RemoveMember_ReturnsNoContent_WhenMemberExists()
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var pm = SeedUser(db);
    var member = new UserEntity { KeycloakId = "member-kc", DisplayName = "Member", Email = "m@example.com" };
    db.Users.Add(member);
    db.SaveChanges();
    var project = SeedProject(db, pm);
    var team = SeedTeam(db, project);
    db.TeamMemberships.Add(new TeamMembershipEntity { TeamId = team.Id, UserId = member.Id, ProjectRole = "Dev", MembershipStatus = MembershipStatus.Invited });
    db.SaveChanges();

    var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

    var response = await client.DeleteAsync($"/api/projects/{project.Id}/teams/{team.Id}/members/{member.Id}");

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    db.ChangeTracker.Clear();
    Assert.False(db.TeamMemberships.Any(tm => tm.TeamId == team.Id && tm.UserId == member.Id));
  }

  [Fact]
  public async Task RemoveMember_ReturnsNotFound_WhenNotMember()
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var pm = SeedUser(db);
    var project = SeedProject(db, pm);
    var team = SeedTeam(db, project);

    var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

    var response = await client.DeleteAsync($"/api/projects/{project.Id}/teams/{team.Id}/members/{Guid.NewGuid()}");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  // -------------------------------------------------------------------------
  // PUT /api/projects/{projectId}/teams/{teamId}/confirm
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ConfirmTeam_ReturnsOk_AndUpdatesProjectStatus()
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var pm = SeedUser(db);
    var project = SeedProject(db, pm, ProjectStatus.Open);
    var team = SeedTeam(db, project, TeamStatus.Proposed);

    var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

    var response = await client.PutAsync($"/api/projects/{project.Id}/teams/{team.Id}/confirm", null);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var dto = await response.Content.ReadFromJsonAsync<TeamDto>(JsonOptions);
    Assert.NotNull(dto);
    Assert.Equal(TeamStatus.Confirmed, dto.Status);

    db.ChangeTracker.Clear();
    Assert.Equal(ProjectStatus.TeamConfirmed, db.Projects.Find(project.Id)!.Status);
  }

  [Fact]
  public async Task ConfirmTeam_ReturnsConflict_WhenNotProposed()
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var pm = SeedUser(db);
    var project = SeedProject(db, pm);
    var team = SeedTeam(db, project, TeamStatus.Rejected);

    var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

    var response = await client.PutAsync($"/api/projects/{project.Id}/teams/{team.Id}/confirm", null);

    Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
  }

  // -------------------------------------------------------------------------
  // PUT /api/projects/{projectId}/teams/{teamId}/reject
  // -------------------------------------------------------------------------

  [Fact]
  public async Task RejectTeam_ReturnsOk_WhenProposed()
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var pm = SeedUser(db);
    var project = SeedProject(db, pm);
    var team = SeedTeam(db, project, TeamStatus.Proposed);

    var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

    var response = await client.PutAsync($"/api/projects/{project.Id}/teams/{team.Id}/reject", null);

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var dto = await response.Content.ReadFromJsonAsync<TeamDto>(JsonOptions);
    Assert.NotNull(dto);
    Assert.Equal(TeamStatus.Rejected, dto.Status);
  }

  [Fact]
  public async Task RejectTeam_ReturnsConflict_WhenAlreadyConfirmed()
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var pm = SeedUser(db);
    var project = SeedProject(db, pm);
    var team = SeedTeam(db, project, TeamStatus.Confirmed);

    var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

    var response = await client.PutAsync($"/api/projects/{project.Id}/teams/{team.Id}/reject", null);

    Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
  }

  // -------------------------------------------------------------------------
  // GET /api/users/me/teams
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GetMyTeams_ReturnsMemberships_ForCallingUser()
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var pm = SeedUser(db);
    var project = SeedProject(db, pm);
    var team = SeedTeam(db, project);
    db.TeamMemberships.Add(new TeamMembershipEntity { TeamId = team.Id, UserId = pm.Id, ProjectRole = "Lead", MembershipStatus = MembershipStatus.Invited });
    db.SaveChanges();

    var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

    var response = await client.GetAsync("/api/users/me/teams");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var memberships = await response.Content.ReadFromJsonAsync<List<UserTeamMembershipDto>>(JsonOptions);
    Assert.NotNull(memberships);
    Assert.Single(memberships);
    Assert.Equal("Lead", memberships[0].ProjectRole);
    Assert.Equal(project.Id, memberships[0].ProjectId);
  }

  [Fact]
  public async Task GetMyTeams_ReturnsEmpty_WhenNoMemberships()
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    SeedUser(db);

    var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

    var response = await client.GetAsync("/api/users/me/teams");

    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var memberships = await response.Content.ReadFromJsonAsync<List<UserTeamMembershipDto>>(JsonOptions);
    Assert.NotNull(memberships);
    Assert.Empty(memberships);
  }

  [Fact]
  public async Task GetMyTeams_ReturnsNotFound_WhenCallerHasNoAppRecord()
  {
    var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

    var response = await client.GetAsync("/api/users/me/teams");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }
}
