namespace SkillMatrixLlm.Api.Models.Emails;

using SkillMatrixLlm.Api.Enums;

/// <summary>Email model for notifying a project manager that a user has responded to a membership invitation.</summary>
/// <param name="UserDisplayName">Display name of the user who responded.</param>
/// <param name="ProjectTitle">Title of the project.</param>
/// <param name="ProjectRole">The role the user was invited for.</param>
/// <param name="Response">The user's response — Accepted or Declined.</param>
public record MembershipResponseEmailModel(
    string UserDisplayName,
    string ProjectTitle,
    string ProjectRole,
    MembershipStatus Response
);
