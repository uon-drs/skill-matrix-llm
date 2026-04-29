namespace SkillMatrixLlm.Api.Data.Entities;

using System.ComponentModel.DataAnnotations.Schema;

public class Recommendation
{
  public Guid Id { get; set; }

  public Guid ProjectId { get; set; }
  public Project? Project { get; set; }

  public Guid TeamId { get; set; }
  public Team? Team { get; set; }

  [Column(TypeName = "jsonb")]
  public required string RawResponse { get; set; }

  public DateTime CreatedAt { get; set; }
}
