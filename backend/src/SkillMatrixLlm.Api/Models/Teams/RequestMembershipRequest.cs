namespace SkillMatrixLlm.Api.Models.Teams;

using System.ComponentModel.DataAnnotations;

/// <summary>Request body for a user self-requesting membership in a team.</summary>
/// <param name="ProjectRole">The role the user wishes to fill within the project.</param>
public record RequestMembershipRequest([Required][MaxLength(256)] string ProjectRole);
