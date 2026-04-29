namespace SkillMatrixLlm.Api.Models;

using SkillMatrixLlm.Api.Enums;
using SkillMatrixLlm.Api.Models.User;

/// <summary>API model representing a user's proficiency in a skill.</summary>
/// <param name="User">The user this skill entry belongs to.</param>
/// <param name="Skill">The skill.</param>
/// <param name="Level">The user's proficiency level.</param>
public record UserSkill(UserDto User, Skill Skill, Level Level);
