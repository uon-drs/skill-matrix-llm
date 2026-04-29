namespace SkillMatrixLlm.Api.Models;

/// <summary>Represents an LLM recommendation audit record.</summary>
/// <param name="Id">Recommendation ID.</param>
/// <param name="ProjectId">ID of the project this recommendation was generated for.</param>
/// <param name="TeamId">ID of the team proposed by this recommendation.</param>
/// <param name="RawResponse">Raw JSON response from the LLM.</param>
/// <param name="CreatedAt">When the recommendation was created.</param>
public record Recommendation(Guid Id, Guid ProjectId, Guid TeamId, string RawResponse, DateTime CreatedAt);
