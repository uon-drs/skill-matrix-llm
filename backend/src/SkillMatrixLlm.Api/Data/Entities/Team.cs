namespace SkillMatrixLlm.Api.Data.Entities;

using SkillMatrixLlm.Api.Enums;

public class Team
{
  public Guid Id { get; set; }

  public Guid ProjectId { get; set; }
  public Project? Project { get; set; }

  public ProjectSource Source { get; set; }

  public TeamStatus Status { get; set; }

  public DateTime CreatedAt { get; set; }
}
