namespace SkillMatrixLlm.Api.Models.User;

using System.ComponentModel.DataAnnotations;
using SkillMatrixLlm.Api.Enums;

/// <summary>Request body for adding a skill to a user's profile.</summary>
/// <param name="SkillId">ID of the skill from the catalogue.</param>
/// <param name="Level">Proficiency level for this skill.</param>
public record AddUserSkillRequest([Required] Guid SkillId, [Required] Level Level);

/// <summary>Request body for updating the proficiency level of an existing skill on a user's profile.</summary>
/// <param name="Level">New proficiency level.</param>
public record UpdateUserSkillLevelRequest([Required] Level Level);
