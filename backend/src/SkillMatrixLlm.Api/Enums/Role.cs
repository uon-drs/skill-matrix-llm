namespace SkillMatrixLlm.Api.Enums;

/// <summary>Application role determining a user's permissions.</summary>
public enum Role
{
  /// <summary>Standard user who can manage their own skills and respond to team invitations.</summary>
  User,
  /// <summary>Can create projects and request LLM team recommendations.</summary>
  ProjectManager,
  /// <summary>Full administrative access including skill catalogue management.</summary>
  Admin
}
