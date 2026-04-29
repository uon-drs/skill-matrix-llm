namespace SkillMatrixLlm.Api.Data.Entities;

using SkillMatrixLlm.Api.Enums;

/// <summary>Join entity linking a user to a team with a role and invitation status.</summary>
public class TeamMembership
{
  /// <summary>Primary key.</summary>
  public Guid Id { get; set; }

  /// <summary>Foreign key to the team.</summary>
  public Guid TeamId { get; set; }
  /// <summary>The team this membership belongs to.</summary>
  public Team? Team { get; set; }

  /// <summary>Foreign key to the user.</summary>
  public Guid UserId { get; set; }
  /// <summary>The user who is a member of the team.</summary>
  public User? User { get; set; }

  /// <summary>The role this user plays within the project.</summary>
  public required string ProjectRole { get; set; }

  /// <summary>Whether the user has been invited, accepted, or declined.</summary>
  public MembershipStatus MembershipStatus { get; set; }
}
