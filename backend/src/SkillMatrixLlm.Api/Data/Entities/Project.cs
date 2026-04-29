namespace SkillMatrixLlm.Api.Data.Entities;

using SkillMatrixLlm.Api.Enums;

/// <summary>A project requiring team assembly.</summary>
public class Project
{
  /// <summary>Primary key.</summary>
  public Guid Id { get; set; }

  /// <summary>Short title of the project.</summary>
  public required string Title { get; set; }

  /// <summary>Full description of the project's goals and requirements.</summary>
  public required string Description { get; set; }

  /// <summary>Target number of team members.</summary>
  public required int DesiredTeamSize { get; set; }

  /// <summary>Expected timeline or duration of the project.</summary>
  public required string Timeline { get; set; }

  /// <summary>Current lifecycle status of the project.</summary>
  public ProjectStatus Status { get; set; }

  /// <summary>Foreign key to the user who created the project.</summary>
  public Guid CreatedByUserId { get; set; }
  /// <summary>The user who created the project.</summary>
  public User? CreatedByUser { get; set; }

  /// <summary>When the project was created.</summary>
  public DateTime CreatedAt { get; set; }
}
