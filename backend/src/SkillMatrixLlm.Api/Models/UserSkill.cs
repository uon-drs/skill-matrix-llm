namespace SkillMatrixLlm.Api.Models;

using SkillMatrixLlm.Api.Enums;
using SkillMatrixLlm.Api.Models.User;

public record UserSkill(UserDto User, Skill Skill, Level Level);
