namespace SkillMatrixLlm.LlmWorker.Models;

/// <summary>A single role and the skills required to fulfil it, as identified by the LLM.</summary>
/// <param name="RoleName">Job title or role label (e.g. "Backend Engineer").</param>
/// <param name="RequiredSkills">Specific technical skills required for the role.</param>
public record RoleRequirement(
  string RoleName,
  List<string> RequiredSkills
);
