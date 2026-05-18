namespace SkillMatrixLlm.LlmWorker.Models;

/// <summary>The structured output produced by the LLM analysis of a project description.</summary>
/// <param name="ProjectId">Identifier of the project this result belongs to.</param>
/// <param name="RawLlmResponse">The raw JSON string returned by the LLM, stored for auditing.</param>
/// <param name="Roles">Parsed list of roles and their required skills.</param>
public record SkillRequirementsResult(
  Guid ProjectId,
  string RawLlmResponse,
  List<RoleRequirement> Roles
);
