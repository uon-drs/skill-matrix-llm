namespace SkillMatrixLlm.Api.Models;

using SkillMatrixLlm.Api.Enums;
using SkillMatrixLlm.Api.Models.User;

public record TeamMembership(Guid Id, UserDto User, string ProjectRole, MembershipStatus MembershipStatus);
