namespace SkillMatrixLlm.Api.Data.Entities;

using SkillMatrixLlm.Api.Enums;

/// <summary>A proposed or confirmed team for a project.</summary>
public class Team
{
  /// <summary>Primary key.</summary>
  public Guid Id { get; set; }

  /// <summary>Foreign key to the project this team was assembled for.</summary>
  public Guid ProjectId { get; set; }
  /// <summary>The project this team was assembled for.</summary>
  public Project? Project { get; set; }

  /// <summary>Whether the team was LLM-generated or manually assembled.</summary>
  public ProjectSource Source { get; set; }

  /// <summary>Current status of the team.</summary>
  public TeamStatus Status { get; set; }

  /// <summary>When the team was created.</summary>
  public DateTime CreatedAt { get; set; }
}
