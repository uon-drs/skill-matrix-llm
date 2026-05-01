namespace SkillMatrixLlm.Api.Tests;

using Data;
using Data.Entities;
using Enums;
using Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Models.Recommendations;
using Xunit;
using ProjectEntity = SkillMatrixLlm.Api.Data.Entities.Project;
using UserEntity = SkillMatrixLlm.Api.Data.Entities.User;

public class TeamBuildingHostedServiceTests(ApiFactory factory) : IClassFixture<ApiFactory>, IAsyncLifetime
{
  public async Task InitializeAsync()
  {
    // Wait for any in-flight messages from a previous test to finish processing
    // before clearing the database, so the hosted service doesn't fail on missing rows.
    await factory.SkillResultsChannel.WaitForIdleAsync();

    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Recommendations.RemoveRange(db.Recommendations);
    db.TeamMemberships.RemoveRange(db.TeamMemberships);
    db.Teams.RemoveRange(db.Teams);
    db.Projects.RemoveRange(db.Projects);
    db.UserSkills.RemoveRange(db.UserSkills);
    db.Skills.RemoveRange(db.Skills);
    db.Users.RemoveRange(db.Users);
    db.SaveChanges();
  }

  public Task DisposeAsync() => Task.CompletedTask;

  private static UserEntity SeedUser(AppDbContext db)
  {
    var user = new UserEntity { KeycloakId = "test-keycloak-id", DisplayName = "Engineer", Email = "eng@example.com" };
    db.Users.Add(user);
    db.SaveChanges();
    return user;
  }

  private static ProjectEntity SeedProject(AppDbContext db, UserEntity user)
  {
    var project = new ProjectEntity
    {
      Title = "Test Project",
      Description = "A project description",
      DesiredTeamSize = 1,
      Timeline = "1 month",
      Status = ProjectStatus.Open,
      CreatedByUserId = user.Id,
      CreatedAt = DateTime.UtcNow,
    };
    db.Projects.Add(project);
    db.SaveChanges();
    return project;
  }

  private static Skill SeedSkill(AppDbContext db, string name)
  {
    var skill = new Skill { Name = name };
    db.Skills.Add(skill);
    db.SaveChanges();
    return skill;
  }

  private static void SeedUserSkill(AppDbContext db, Guid userId, Guid skillId)
  {
    db.UserSkills.Add(new UserSkill { UserId = userId, SkillId = skillId, Level = Level.Intermediate });
    db.SaveChanges();
  }

  private static async Task<Recommendation?> WaitForRecommendation(ApiFactory factory, Guid projectId)
  {
    var deadline = DateTimeOffset.UtcNow.AddSeconds(5);
    while (DateTimeOffset.UtcNow < deadline)
    {
      await Task.Delay(50);
      using var scope = factory.Services.CreateScope();
      var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
      var recommendation = await db.Recommendations.FirstOrDefaultAsync(r => r.ProjectId == projectId);
      if (recommendation is not null)
      {
        return recommendation;
      }
    }

    return null;
  }

  // -------------------------------------------------------------------------
  // ExecuteAsync
  // -------------------------------------------------------------------------

  [Fact]
  public async Task ExecuteAsync_CreatesProposedTeamAndRecommendation_WhenResultIsReceived()
  {
    using var setup = factory.Services.CreateScope();
    var db = setup.ServiceProvider.GetRequiredService<AppDbContext>();
    var user = SeedUser(db);
    var project = SeedProject(db, user);
    var skill = SeedSkill(db, "Python");
    SeedUserSkill(db, user.Id, skill.Id);

    factory.SkillResultsChannel.Enqueue(new SkillRequirementsResult(
      project.Id,
      """{"roles":[{"role":"Backend Engineer","skills":["Python"]}]}""",
      [new RoleRequirement("Backend Engineer", ["Python"])]));

    var recommendation = await WaitForRecommendation(factory, project.Id);

    Assert.NotNull(recommendation);
    Assert.Equal(project.Id, recommendation.ProjectId);

    using var assert = factory.Services.CreateScope();
    var assertDb = assert.ServiceProvider.GetRequiredService<AppDbContext>();

    var team = await assertDb.Teams.FindAsync(recommendation.TeamId);
    Assert.NotNull(team);
    Assert.Equal(ProjectSource.LlmGenerated, team.Source);
    Assert.Equal(TeamStatus.Proposed, team.Status);
    Assert.Equal(project.Id, team.ProjectId);

    var memberships = await assertDb.TeamMemberships
      .Where(tm => tm.TeamId == team.Id)
      .ToListAsync();
    Assert.Single(memberships);
    Assert.Equal(user.Id, memberships[0].UserId);
    Assert.Equal("Backend Engineer", memberships[0].ProjectRole);
    Assert.Equal(MembershipStatus.Invited, memberships[0].MembershipStatus);
  }

  [Fact]
  public async Task ExecuteAsync_CreatesTeamWithNoMembers_WhenNoUsersMatchRequiredSkills()
  {
    using var setup = factory.Services.CreateScope();
    var db = setup.ServiceProvider.GetRequiredService<AppDbContext>();
    var user = SeedUser(db);
    var project = SeedProject(db, user);
    // No skills seeded for any user.

    factory.SkillResultsChannel.Enqueue(new SkillRequirementsResult(
      project.Id,
      """{}""",
      [new RoleRequirement("Backend Engineer", ["Python"])]));

    var recommendation = await WaitForRecommendation(factory, project.Id);

    Assert.NotNull(recommendation);

    using var assert = factory.Services.CreateScope();
    var assertDb = assert.ServiceProvider.GetRequiredService<AppDbContext>();
    var memberships = await assertDb.TeamMemberships
      .Where(tm => tm.TeamId == recommendation.TeamId)
      .ToListAsync();
    Assert.Empty(memberships);
  }
}
