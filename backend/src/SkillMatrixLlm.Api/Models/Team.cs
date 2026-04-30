namespace SkillMatrixLlm.Api.Models;

using SkillMatrixLlm.Api.Enums;

/// <summary>API model representing a team assembled for a project.</summary>
/// <param name="Id">Team ID.</param>
/// <param name="Source">Whether the team was LLM-generated or manually assembled.</param>
/// <param name="Status">Current status of the team proposal.</param>
/// <param name="CreatedAt">When the team was created.</param>
/// <param name="Members">Team membership records.</param>
public record TeamDto(Guid Id, ProjectSource Source, TeamStatus Status, DateTime CreatedAt, List<TeamMembership> Members);
