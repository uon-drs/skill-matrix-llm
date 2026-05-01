namespace SkillMatrixLlm.Api.Models.Recommendations;

/// <summary>A single role and the skills required to fill it, as determined by the LLM.</summary>
/// <param name="RoleName">Human-readable role name (e.g. "Backend Engineer").</param>
/// <param name="RequiredSkills">Skills required for this role.</param>
public record RoleRequirement(string RoleName, List<string> RequiredSkills);
