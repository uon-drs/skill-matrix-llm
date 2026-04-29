namespace SkillMatrixLlm.Api.Data.Entities;

using SkillMatrixLlm.Api.Enums;

public class Project
{
  public Guid Id { get; set; }

  public required string Title { get; set; }

  public required string Description { get; set; }

  public required int DesiredTeamSize { get; set; }

  public required string Timeline { get; set; }

  public ProjectStatus Status { get; set; }

  public Guid CreatedByUserId { get; set; }
  public User? CreatedByUser { get; set; }

  public DateTime CreatedAt { get; set; }
}
