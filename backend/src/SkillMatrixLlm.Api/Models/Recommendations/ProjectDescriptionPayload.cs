namespace SkillMatrixLlm.Api.Models.Recommendations;

/// <summary>
/// Payload dispatched to the LLM service when a recommendation is triggered.
/// Only project description data is included — user skill data never leaves the backend.
/// </summary>
/// <param name="ProjectId">ID of the project requiring a team recommendation.</param>
/// <param name="Title">Project title.</param>
/// <param name="Description">Full project description.</param>
/// <param name="TeamSize">Desired number of team members.</param>
/// <param name="Timeline">Expected project timeline.</param>
public record ProjectDescriptionPayload(
  Guid ProjectId,
  string Title,
  string Description,
  int TeamSize,
  string Timeline
);
