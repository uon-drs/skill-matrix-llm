namespace SkillMatrixLlm.Api.Models.Teams;

using System.ComponentModel.DataAnnotations;
using SkillMatrixLlm.Api.Enums;

/// <summary>Request body for creating a new team for a project.</summary>
/// <param name="Source">Whether the team is LLM-generated or manually assembled.</param>
public record CreateTeamRequest([Required] ProjectSource Source);
