namespace SkillMatrixLlm.LlmWorker.Models;

/// <summary>Describes a project for which the LLM should identify required roles and skills.</summary>
/// <param name="ProjectId">Unique identifier of the project.</param>
/// <param name="Title">Short display name for the project.</param>
/// <param name="Description">Free-text description of the project's goals and technology.</param>
/// <param name="TeamSize">Number of roles the LLM should identify.</param>
/// <param name="Timeline">Expected project duration, used to calibrate generalist vs specialist recommendations.</param>
public record ProjectDescriptionPayload(
  Guid ProjectId,
  string Title,
  string Description,
  int TeamSize,
  string Timeline
);
