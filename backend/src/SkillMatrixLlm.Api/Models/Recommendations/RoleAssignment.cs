namespace SkillMatrixLlm.Api.Models.Recommendations;

/// <summary>The outcome of matching a candidate to an LLM-proposed role.</summary>
/// <param name="UserId">Application user ID of the matched candidate.</param>
/// <param name="RoleName">The role name the candidate was assigned to.</param>
public record RoleAssignment(Guid UserId, string RoleName);
