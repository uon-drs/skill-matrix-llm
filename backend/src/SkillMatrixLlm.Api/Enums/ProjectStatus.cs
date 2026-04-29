namespace SkillMatrixLlm.Api.Enums;

/// <summary>Lifecycle status of a project.</summary>
public enum ProjectStatus
{
  /// <summary>The project is being drafted and is not yet open for team assembly.</summary>
  Draft,
  /// <summary>The project is open and a team is being assembled.</summary>
  Open,
  /// <summary>A team has been confirmed for the project.</summary>
  TeamConfirmed,
  /// <summary>The project is closed.</summary>
  Closed
}
