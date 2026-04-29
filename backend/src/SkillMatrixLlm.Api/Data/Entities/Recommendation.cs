namespace SkillMatrixLlm.Api.Data.Entities;

using System.ComponentModel.DataAnnotations.Schema;

/// <summary>Audit record storing a raw LLM team recommendation.</summary>
public class Recommendation
{
  /// <summary>Primary key.</summary>
  public Guid Id { get; set; }

  /// <summary>Foreign key to the project the recommendation was generated for.</summary>
  public Guid ProjectId { get; set; }
  /// <summary>The project this recommendation was generated for.</summary>
  public Project? Project { get; set; }

  /// <summary>Foreign key to the team proposed by this recommendation.</summary>
  public Guid TeamId { get; set; }
  /// <summary>The team proposed by this recommendation.</summary>
  public Team? Team { get; set; }

  /// <summary>Raw JSON response from the LLM.</summary>
  [Column(TypeName = "jsonb")]
  public required string RawResponse { get; set; }

  /// <summary>When the recommendation was generated.</summary>
  public DateTime CreatedAt { get; set; }
}
