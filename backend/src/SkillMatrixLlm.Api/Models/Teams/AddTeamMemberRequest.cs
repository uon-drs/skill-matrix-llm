namespace SkillMatrixLlm.Api.Models.Teams;

using System.ComponentModel.DataAnnotations;

/// <summary>Request body for adding a member to a team.</summary>
/// <param name="UserId">Application user ID of the member to add.</param>
/// <param name="ProjectRole">The role this user will play within the project.</param>
public record AddTeamMemberRequest([Required] Guid UserId, [Required][MaxLength(256)] string ProjectRole);
