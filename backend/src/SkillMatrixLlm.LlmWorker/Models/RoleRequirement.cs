namespace SkillMatrixLlm.LlmWorker.Models;

public record RoleRequirement(
  string RoleName,
  List<string> RequiredSkills
);
