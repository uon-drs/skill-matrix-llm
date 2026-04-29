namespace SkillMatrixLlm.Api.Data.Entities;

using SkillMatrixLlm.Api.Enums;

public class TeamMembership
{
  public Guid Id { get; set; }

  public Guid TeamId { get; set; }
  public Team? Team { get; set; }

  public Guid UserId { get; set; }
  public User? User { get; set; }

  public required string ProjectRole { get; set; }

  public MembershipStatus MembershipStatus { get; set; }
}
