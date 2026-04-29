namespace SkillMatrixLlm.Api.Enums;

/// <summary>Invitation status of a user's team membership.</summary>
public enum MembershipStatus
{
  /// <summary>The user has been invited but not yet responded.</summary>
  Invited,
  /// <summary>The user has accepted the invitation.</summary>
  Accepted,
  /// <summary>The user has declined the invitation.</summary>
  Declined
}
