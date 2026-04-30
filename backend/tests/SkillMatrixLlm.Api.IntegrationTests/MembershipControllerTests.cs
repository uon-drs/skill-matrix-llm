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
using Xunit;
using ProjectEntity = SkillMatrixLlm.Api.Data.Entities.Project;
using TeamEntity = SkillMatrixLlm.Api.Data.Entities.Team;
using TeamMembershipEntity = SkillMatrixLlm.Api.Data.Entities.TeamMembership;
using UserEntity = SkillMatrixLlm.Api.Data.Entities.User;

public class MembershipControllerTests(ApiFactory factory) : IClassFixture<ApiFactory>, IAsyncLifetime
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

    private (UserEntity user, ProjectEntity project, TeamEntity team) SeedOpenProjectWithProposedTeam(AppDbContext db, string keycloakId = "test-keycloak-id")
    {
        var user = new UserEntity { KeycloakId = keycloakId, DisplayName = "PM", Email = "pm@example.com" };
        db.Users.Add(user);
        db.SaveChanges();

        var project = new ProjectEntity
        {
            Title = "Test Project",
            Description = "Desc",
            DesiredTeamSize = 3,
            Timeline = "3 months",
            Status = ProjectStatus.Open,
            CreatedByUserId = user.Id,
            CreatedAt = DateTime.UtcNow,
        };
        db.Projects.Add(project);
        db.SaveChanges();

        var team = new TeamEntity
        {
            ProjectId = project.Id,
            Source = ProjectSource.ManuallyAssembled,
            Status = TeamStatus.Proposed,
            CreatedAt = DateTime.UtcNow,
        };
        db.Teams.Add(team);
        db.SaveChanges();

        return (user, project, team);
    }

    // -------------------------------------------------------------------------
    // POST /api/projects/{projectId}/teams/{teamId}/membership-requests
    // -------------------------------------------------------------------------

    [Fact]
    public async Task RequestMembership_ReturnsCreated_WhenTeamIsProposedAndProjectIsOpen()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (_, project, team) = SeedOpenProjectWithProposedTeam(db);

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PostAsJsonAsync(
            $"/api/users/me/membership-requests/{team.Id}",
            new { ProjectRole = "Developer" },
            RequestJsonOptions);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var membership = await response.Content.ReadFromJsonAsync<TeamMembership>(JsonOptions);
        Assert.NotNull(membership);
        Assert.Equal("Developer", membership.ProjectRole);
        Assert.Equal(MembershipStatus.Requested, membership.MembershipStatus);
    }

    [Fact]
    public async Task RequestMembership_ReturnsConflict_WhenTeamIsConfirmed()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (_, project, team) = SeedOpenProjectWithProposedTeam(db);
        team.Status = TeamStatus.Confirmed;
        db.SaveChanges();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PostAsJsonAsync(
            $"/api/users/me/membership-requests/{team.Id}",
            new { ProjectRole = "Developer" },
            RequestJsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task RequestMembership_ReturnsConflict_WhenUserAlreadyActiveOnAnotherTeamInProject()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (user, project, team) = SeedOpenProjectWithProposedTeam(db);

        var otherTeam = new TeamEntity { ProjectId = project.Id, Source = ProjectSource.ManuallyAssembled, Status = TeamStatus.Proposed, CreatedAt = DateTime.UtcNow };
        db.Teams.Add(otherTeam);
        db.SaveChanges();
        db.TeamMemberships.Add(new TeamMembershipEntity { TeamId = otherTeam.Id, UserId = user.Id, ProjectRole = "Lead", MembershipStatus = MembershipStatus.Invited });
        db.SaveChanges();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PostAsJsonAsync(
            $"/api/users/me/membership-requests/{team.Id}",
            new { ProjectRole = "Developer" },
            RequestJsonOptions);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task RequestMembership_ReturnsNotFound_WhenCallerHasNoAppRecord()
    {
        // No user seeded — sub claim has no matching app record
        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PostAsJsonAsync(
            $"/api/users/me/membership-requests/{Guid.NewGuid()}",
            new { ProjectRole = "Developer" },
            RequestJsonOptions);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // PUT /api/users/me/memberships/{teamId}/accept
    // -------------------------------------------------------------------------

    [Fact]
    public async Task AcceptMembership_ReturnsOk_WhenMembershipIsInvited()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (user, _, team) = SeedOpenProjectWithProposedTeam(db);
        db.TeamMemberships.Add(new TeamMembershipEntity { TeamId = team.Id, UserId = user.Id, ProjectRole = "Dev", MembershipStatus = MembershipStatus.Invited });
        db.SaveChanges();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PutAsync($"/api/users/me/memberships/{team.Id}/accept", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var membership = await response.Content.ReadFromJsonAsync<TeamMembership>(JsonOptions);
        Assert.NotNull(membership);
        Assert.Equal(MembershipStatus.Accepted, membership.MembershipStatus);
    }

    [Fact]
    public async Task AcceptMembership_ReturnsOk_WhenMembershipIsRequested()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (user, _, team) = SeedOpenProjectWithProposedTeam(db);
        db.TeamMemberships.Add(new TeamMembershipEntity { TeamId = team.Id, UserId = user.Id, ProjectRole = "Dev", MembershipStatus = MembershipStatus.Requested });
        db.SaveChanges();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PutAsync($"/api/users/me/memberships/{team.Id}/accept", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AcceptMembership_ReturnsConflict_WhenAlreadyAccepted()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (user, _, team) = SeedOpenProjectWithProposedTeam(db);
        db.TeamMemberships.Add(new TeamMembershipEntity { TeamId = team.Id, UserId = user.Id, ProjectRole = "Dev", MembershipStatus = MembershipStatus.Accepted });
        db.SaveChanges();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PutAsync($"/api/users/me/memberships/{team.Id}/accept", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task AcceptMembership_ReturnsNotFound_WhenNoMembership()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        SeedOpenProjectWithProposedTeam(db);

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PutAsync($"/api/users/me/memberships/{Guid.NewGuid()}/accept", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // -------------------------------------------------------------------------
    // PUT /api/users/me/memberships/{teamId}/decline
    // -------------------------------------------------------------------------

    [Fact]
    public async Task DeclineMembership_ReturnsOk_WhenMembershipIsInvited()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (user, _, team) = SeedOpenProjectWithProposedTeam(db);
        db.TeamMemberships.Add(new TeamMembershipEntity { TeamId = team.Id, UserId = user.Id, ProjectRole = "Dev", MembershipStatus = MembershipStatus.Invited });
        db.SaveChanges();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PutAsync($"/api/users/me/memberships/{team.Id}/decline", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var membership = await response.Content.ReadFromJsonAsync<TeamMembership>(JsonOptions);
        Assert.NotNull(membership);
        Assert.Equal(MembershipStatus.Declined, membership.MembershipStatus);
    }

    [Fact]
    public async Task DeclineMembership_ReturnsConflict_WhenAlreadyDeclined()
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var (user, _, team) = SeedOpenProjectWithProposedTeam(db);
        db.TeamMemberships.Add(new TeamMembershipEntity { TeamId = team.Id, UserId = user.Id, ProjectRole = "Dev", MembershipStatus = MembershipStatus.Declined });
        db.SaveChanges();

        var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

        var response = await client.PutAsync($"/api/users/me/memberships/{team.Id}/decline", null);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
