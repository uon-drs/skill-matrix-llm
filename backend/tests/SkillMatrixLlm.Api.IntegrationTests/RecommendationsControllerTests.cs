namespace SkillMatrixLlm.Api.Tests;

using System.Net;
using Constants;
using Data;
using Enums;
using Fixtures;
using Messaging;
using Microsoft.Extensions.DependencyInjection;
using Models.Recommendations;
using Moq;
using Xunit;
using ProjectEntity = SkillMatrixLlm.Api.Data.Entities.Project;
using UserEntity = SkillMatrixLlm.Api.Data.Entities.User;

public class RecommendationsControllerTests(ApiFactory factory) : IClassFixture<ApiFactory>, IAsyncLifetime
{
  public Task InitializeAsync()
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Recommendations.RemoveRange(db.Recommendations);
    db.TeamMemberships.RemoveRange(db.TeamMemberships);
    db.Teams.RemoveRange(db.Teams);
    db.Projects.RemoveRange(db.Projects);
    db.Users.RemoveRange(db.Users);
    db.SaveChanges();

    Mock.Get(factory.Services.GetRequiredService<IMessageChannel<ProjectDescriptionPayload>>()).Reset();

    return Task.CompletedTask;
  }

  public Task DisposeAsync() => Task.CompletedTask;

  private static UserEntity SeedUser(AppDbContext db)
  {
    var user = new UserEntity { KeycloakId = "test-keycloak-id", DisplayName = "PM", Email = "pm@example.com" };
    db.Users.Add(user);
    db.SaveChanges();
    return user;
  }

  private static ProjectEntity SeedProject(AppDbContext db, UserEntity user, ProjectStatus status)
  {
    var project = new ProjectEntity
    {
      Title = "Test Project",
      Description = "A project description",
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

  // -------------------------------------------------------------------------
  // POST /api/projects/{projectId}/recommendations
  // -------------------------------------------------------------------------

  [Theory]
  [InlineData(ProjectStatus.Draft)]
  [InlineData(ProjectStatus.Open)]
  public async Task Trigger_ReturnsAccepted_WhenProjectIsInAllowedStatus(ProjectStatus status)
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var user = SeedUser(db);
    var project = SeedProject(db, user, status);

    var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

    var response = await client.PostAsync($"/api/projects/{project.Id}/recommendations", null);

    Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
  }

  [Theory]
  [InlineData(ProjectStatus.Draft)]
  [InlineData(ProjectStatus.Open)]
  public async Task Trigger_PublishesPayload_WhenProjectIsInAllowedStatus(ProjectStatus status)
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var user = SeedUser(db);
    var project = SeedProject(db, user, status);

    var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

    await client.PostAsync($"/api/projects/{project.Id}/recommendations", null);

    var queue = factory.Services.GetRequiredService<IMessageChannel<ProjectDescriptionPayload>>();
    Mock.Get(queue).Verify(
      q => q.PublishAsync(
        It.Is<ProjectDescriptionPayload>(p => p.ProjectId == project.Id),
        It.IsAny<CancellationToken>()),
      Times.Once);
  }

  [Fact]
  public async Task Trigger_ReturnsNotFound_WhenProjectDoesNotExist()
  {
    var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

    var response = await client.PostAsync($"/api/projects/{Guid.NewGuid()}/recommendations", null);

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Theory]
  [InlineData(ProjectStatus.TeamConfirmed)]
  [InlineData(ProjectStatus.Closed)]
  public async Task Trigger_ReturnsConflict_WhenProjectStatusDisallowsRecommendations(ProjectStatus status)
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var user = SeedUser(db);
    var project = SeedProject(db, user, status);

    var client = factory.CreateAuthenticatedClient([TestClaims.Sub, TestClaims.ManageProjectsRole]);

    var response = await client.PostAsync($"/api/projects/{project.Id}/recommendations", null);

    Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
  }

  [Fact]
  public async Task Trigger_ReturnsForbidden_WithoutManageProjectsRole()
  {
    var client = factory.CreateAuthenticatedClient([TestClaims.Sub]);

    var response = await client.PostAsync($"/api/projects/{Guid.NewGuid()}/recommendations", null);

    Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
  }
}
