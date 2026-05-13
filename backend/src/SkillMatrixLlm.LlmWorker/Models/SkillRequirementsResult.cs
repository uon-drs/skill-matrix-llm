namespace SkillMatrixLlm.LlmWorker.Models;

public record SkillRequirementsResult(
  Guid ProjectId,
  string RawLlmResponse,
  List<RoleRequirement> Roles
);
