namespace SkillMatrixLlm.Api.Models.Recommendations;

/// <summary>
/// Result returned by the LLM service after analysing a project description.
/// Consumed by <c>TeamBuildingHostedService</c> to match staff and persist the recommendation.
/// </summary>
/// <param name="ProjectId">ID of the project this result relates to.</param>
/// <param name="RawLlmResponse">The raw JSON response from the LLM, stored verbatim for audit purposes.</param>
/// <param name="Roles">Structured list of roles and the skills required to fill each one.</param>
public record SkillRequirementsResult(
    Guid ProjectId,
    string RawLlmResponse,
    List<RoleRequirement> Roles
);
