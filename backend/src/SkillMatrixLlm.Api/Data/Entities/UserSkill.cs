namespace SkillMatrixLlm.Api.Data.Entities;

using SkillMatrixLlm.Api.Enums;

/// <summary>Join entity representing a user's proficiency in a skill.</summary>
public class UserSkill
{
  /// <summary>Primary key.</summary>
  public Guid Id { get; set; }

  /// <summary>Foreign key to the user.</summary>
  public Guid UserId { get; set; }
  /// <summary>The user this skill entry belongs to.</summary>
  public User? User { get; set; }

  /// <summary>Foreign key to the skill.</summary>
  public Guid SkillId { get; set; }
  /// <summary>The skill this entry describes.</summary>
  public Skill? Skill { get; set; }

  /// <summary>The user's proficiency level in this skill.</summary>
  public Level Level { get; set; }
}
