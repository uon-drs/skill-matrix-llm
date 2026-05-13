namespace SkillMatrixLlm.LlmWorker.Models;

public record ProjectDescriptionPayload(
  Guid ProjectId,
  string Title,
  string Description,
  int TeamSize,
  string Timeline
);
