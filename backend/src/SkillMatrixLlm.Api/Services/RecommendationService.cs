namespace SkillMatrixLlm.Api.Services;

using Data;
using Enums;
using Messaging;
using Microsoft.EntityFrameworkCore;
using Models.Recommendations;

/// <summary>Dispatches team recommendation requests for projects to the LLM service.</summary>
public class RecommendationService(AppDbContext db, IMessageChannel<ProjectDescriptionPayload> queue)
{
  /// <summary>
  /// Publishes a team recommendation request for the given project.
  /// </summary>
  /// <param name="projectId">ID of the project to recommend a team for.</param>
  /// <param name="ct">Cancellation token.</param>
  /// <exception cref="KeyNotFoundException">Thrown when the project does not exist.</exception>
  /// <exception cref="InvalidOperationException">Thrown when the project status does not allow recommendations.</exception>
  public async Task TriggerAsync(Guid projectId, CancellationToken ct = default)
  {
    var project = await db.Projects.FirstOrDefaultAsync(p => p.Id == projectId, ct)
        ?? throw new KeyNotFoundException($"Project {projectId} not found.");

    if (project.Status is ProjectStatus.TeamConfirmed or ProjectStatus.Closed)
    {
      throw new InvalidOperationException(
        $"Cannot request a recommendation for a project with status {project.Status}.");
    }

    var payload = new ProjectDescriptionPayload(
      project.Id,
      project.Title,
      project.Description,
      project.DesiredTeamSize,
      project.Timeline);

    await queue.PublishAsync(payload, ct);
  }
}
