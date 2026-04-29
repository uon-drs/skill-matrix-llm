namespace SkillMatrixLlm.Api.Models.User;

using SkillMatrixLlm.Api.Enums;

public record UserDto(Guid Id, string DisplayName, string Email, Role Role);
