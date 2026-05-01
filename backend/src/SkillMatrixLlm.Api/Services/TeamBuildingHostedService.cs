namespace SkillMatrixLlm.Api.Services;

using Data;
using Data.Entities;
using Enums;
using Messaging;
using Models.Recommendations;

/// <summary>
/// Background service that consumes LLM skill-requirements results, matches candidates
/// to proposed roles, and persists the resulting team and recommendation record.
/// </summary>
public class TeamBuildingHostedService(
  IMessageChannel<SkillRequirementsResult> queue,
  IServiceScopeFactory scopeFactory,
  ILogger<TeamBuildingHostedService> logger) : BackgroundService
{
  private static readonly Action<ILogger, Guid, Exception?> _logProcessing =
    LoggerMessage.Define<Guid>(
      LogLevel.Information, new EventId(1), "Processing skill requirements for project {ProjectId}");

  private static readonly Action<ILogger, Guid, Exception> _logProcessingError =
    LoggerMessage.Define<Guid>(
      LogLevel.Error, new EventId(2), "Failed to process skill requirements for project {ProjectId}");

  /// <inheritdoc />
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    await foreach (var result in queue.ConsumeAsync(stoppingToken))
    {
      try
      {
        await ProcessResultAsync(result, stoppingToken);
      }
      catch (Exception ex)
      {
        _logProcessingError(logger, result.ProjectId, ex);
      }
    }
  }

  private async Task ProcessResultAsync(SkillRequirementsResult result, CancellationToken ct)
  {
    _logProcessing(logger, result.ProjectId, null);

    await using var scope = scopeFactory.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var matcher = scope.ServiceProvider.GetRequiredService<StaffMatchingService>();

    var assignments = await matcher.MatchAsync(result.Roles, ct);

    var team = new Team
    {
      ProjectId = result.ProjectId,
      Source = ProjectSource.LlmGenerated,
      Status = TeamStatus.Proposed,
      CreatedAt = DateTime.UtcNow,
    };
    db.Teams.Add(team);
    await db.SaveChangesAsync(ct);

    foreach (var assignment in assignments)
    {
      db.TeamMemberships.Add(new TeamMembership
      {
        TeamId = team.Id,
        UserId = assignment.UserId,
        ProjectRole = assignment.RoleName,
        MembershipStatus = MembershipStatus.Invited,
      });
    }

    db.Recommendations.Add(new Recommendation
    {
      ProjectId = result.ProjectId,
      TeamId = team.Id,
      RawResponse = result.RawLlmResponse,
      CreatedAt = DateTime.UtcNow,
    });

    await db.SaveChangesAsync(ct);
  }
}
