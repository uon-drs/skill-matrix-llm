namespace SkillMatrixLlm.Api.Models.User;

using SkillMatrixLlm.Api.Enums;

/// <summary>Lightweight user representation for embedding in other API models.</summary>
/// <param name="Id">User ID.</param>
/// <param name="DisplayName">Display name shown in the UI.</param>
/// <param name="Email">Email address.</param>
/// <param name="Role">Application role.</param>
public record UserDto(Guid Id, string DisplayName, string Email, Role Role);
