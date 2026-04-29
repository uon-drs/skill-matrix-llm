namespace SkillMatrixLlm.Api.Models;

using SkillMatrixLlm.Api.Enums;
using SkillMatrixLlm.Api.Models.User;

/// <summary>API model representing a user's membership in a team.</summary>
/// <param name="Id">Membership ID.</param>
/// <param name="User">The member.</param>
/// <param name="ProjectRole">The role this user plays within the project.</param>
/// <param name="MembershipStatus">Whether the user has been invited, accepted, or declined.</param>
public record TeamMembership(Guid Id, UserDto User, string ProjectRole, MembershipStatus MembershipStatus);
