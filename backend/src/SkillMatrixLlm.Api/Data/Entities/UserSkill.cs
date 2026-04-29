namespace SkillMatrixLlm.Api.Data.Entities;

using SkillMatrixLlm.Api.Enums;

public class UserSkill
{
  public Guid Id { get; set; }

  public Guid UserId { get; set; }
  public User? User { get; set; }

  public Guid SkillId { get; set; }
  public Skill? Skill { get; set; }

  public Level Level { get; set; }
}
