namespace SkillMatrixLlm.Api.Models.User;

using SkillMatrixLlm.Api.Enums;

/// <summary>A user's proficiency in a single skill.</summary>
/// <param name="SkillId">Skill ID.</param>
/// <param name="SkillName">Display name of the skill.</param>
/// <param name="Level">Proficiency level.</param>
public record UserSkillDto(Guid SkillId, string SkillName, Level Level);

/// <summary>Full user profile including skill proficiencies.</summary>
/// <param name="Id">Application user ID.</param>
/// <param name="DisplayName">Display name shown in the UI.</param>
/// <param name="Email">Email address.</param>
/// <param name="Role">Application role.</param>
/// <param name="Skills">Skill proficiency records for this user.</param>
public record UserProfileDto(Guid Id, string DisplayName, string Email, Role Role, List<UserSkillDto> Skills);

/// <summary>Lightweight user record for list views.</summary>
/// <param name="Id">Application user ID.</param>
/// <param name="DisplayName">Display name shown in the UI.</param>
/// <param name="Email">Email address.</param>
/// <param name="Role">Application role.</param>
public record UserSummaryDto(Guid Id, string DisplayName, string Email, Role Role);

/// <summary>Request body for updating a user's application role.</summary>
/// <param name="Role">The new role to assign.</param>
public record SetRoleRequest(Role Role);
