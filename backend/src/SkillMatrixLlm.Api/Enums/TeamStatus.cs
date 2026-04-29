namespace SkillMatrixLlm.Api.Enums;

/// <summary>Status of a team proposal for a project.</summary>
public enum TeamStatus
{
  /// <summary>The team has been proposed but not yet confirmed.</summary>
  Proposed,
  /// <summary>The team has been confirmed for the project.</summary>
  Confirmed,
  /// <summary>The team proposal was rejected.</summary>
  Rejected
}
