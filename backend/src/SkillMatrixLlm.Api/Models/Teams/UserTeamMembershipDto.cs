namespace SkillMatrixLlm.Api.Models.Teams;

using SkillMatrixLlm.Api.Enums;

/// <summary>A user's membership in a team, with enough project context to be useful in a list view.</summary>
/// <param name="TeamId">Team ID.</param>
/// <param name="ProjectId">ID of the project the team belongs to.</param>
/// <param name="ProjectTitle">Title of the project.</param>
/// <param name="ProjectStatus">Current status of the project.</param>
/// <param name="TeamStatus">Current status of the team.</param>
/// <param name="ProjectRole">The role this user plays within the project.</param>
/// <param name="MembershipStatus">Whether the user has been invited, accepted, or declined.</param>
public record UserTeamMembershipDto(
    Guid TeamId,
    Guid ProjectId,
    string ProjectTitle,
    ProjectStatus ProjectStatus,
    TeamStatus TeamStatus,
    string ProjectRole,
    MembershipStatus MembershipStatus
);
