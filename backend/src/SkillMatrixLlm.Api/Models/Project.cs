namespace SkillMatrixLlm.Api.Models;

using SkillMatrixLlm.Api.Enums;
using SkillMatrixLlm.Api.Models.User;

public record Project(
  Guid Id,
  string Title,
  string Description,
  int DesiredTeamSize,
  string Timeline,
  ProjectStatus Status,
  UserDto CreatedByUser,
  DateTime CreatedAt
);
