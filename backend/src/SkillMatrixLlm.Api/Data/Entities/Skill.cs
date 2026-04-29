namespace SkillMatrixLlm.Api.Data.Entities;

/// <summary>A skill in the curated catalogue.</summary>
public class Skill
{
  /// <summary>Primary key.</summary>
  public Guid Id { get; set; }

  /// <summary>Display name of the skill.</summary>
  public required string Name { get; set; }
}
