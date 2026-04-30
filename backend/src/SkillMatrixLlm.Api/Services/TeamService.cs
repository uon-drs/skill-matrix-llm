namespace SkillMatrixLlm.Api.Services;

using Data;
using Enums;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Teams;
using Models.User;
using TeamEntity = Data.Entities.Team;
using TeamMembershipEntity = Data.Entities.TeamMembership;

/// <summary>Manages teams and their membership for projects.</summary>
public class TeamService(AppDbContext db)
{
    /// <summary>
    /// Creates a new proposed team for a project.
    /// </summary>
    /// <param name="projectId">Project ID.</param>
    /// <param name="source">Whether the team is LLM-generated or manually assembled.</param>
    /// <returns>The created team.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the project does not exist.</exception>
    public async Task<TeamDto> CreateAsync(Guid projectId, ProjectSource source)
    {
        var exists = await db.Projects.AnyAsync(p => p.Id == projectId);
        if (!exists)
            throw new KeyNotFoundException($"Project {projectId} not found.");

        var team = new TeamEntity
        {
            ProjectId = projectId,
            Source = source,
            Status = TeamStatus.Proposed,
            CreatedAt = DateTime.UtcNow,
        };

        db.Teams.Add(team);
        await db.SaveChangesAsync();

        return new TeamDto(team.Id, team.Source, team.Status, team.CreatedAt, []);
    }

    /// <summary>
    /// Adds a user to a team with a specified project role.
    /// </summary>
    /// <param name="teamId">Team ID.</param>
    /// <param name="userId">Application user ID of the member to add.</param>
    /// <param name="projectRole">The role this user plays within the project.</param>
    /// <returns>The created membership record.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the team or user does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the user is already a member of the team.</exception>
    public async Task<TeamMembership> AddMemberAsync(Guid teamId, Guid userId, string projectRole)
    {
        var teamExists = await db.Teams.AnyAsync(t => t.Id == teamId);
        if (!teamExists)
            throw new KeyNotFoundException($"Team {teamId} not found.");

        var user = await db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException($"User {userId} not found.");

        var alreadyMember = await db.TeamMemberships.AnyAsync(tm => tm.TeamId == teamId && tm.UserId == userId);
        if (alreadyMember)
            throw new InvalidOperationException($"User {userId} is already a member of team {teamId}.");

        var membership = new TeamMembershipEntity
        {
            TeamId = teamId,
            UserId = userId,
            ProjectRole = projectRole,
            MembershipStatus = MembershipStatus.Invited,
        };

        db.TeamMemberships.Add(membership);
        await db.SaveChangesAsync();

        return new TeamMembership(membership.Id, new UserDto(user.Id, user.DisplayName, user.Email, user.Role), projectRole, MembershipStatus.Invited);
    }

    /// <summary>
    /// Removes a user from a team.
    /// </summary>
    /// <param name="teamId">Team ID.</param>
    /// <param name="userId">Application user ID of the member to remove.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the membership record does not exist.</exception>
    public async Task RemoveMemberAsync(Guid teamId, Guid userId)
    {
        var membership = await db.TeamMemberships.FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId)
            ?? throw new KeyNotFoundException($"User {userId} is not a member of team {teamId}.");

        db.TeamMemberships.Remove(membership);
        await db.SaveChangesAsync();
    }

    /// <summary>
    /// Confirms a proposed team, transitioning it to Confirmed status and updating the parent
    /// project to TeamConfirmed if it is currently Open.
    /// </summary>
    /// <param name="teamId">Team ID.</param>
    /// <returns>The confirmed team.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the team does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the team is not in Proposed status.</exception>
    public async Task<TeamDto> ConfirmAsync(Guid teamId)
    {
        var team = await db.Teams.FindAsync(teamId)
            ?? throw new KeyNotFoundException($"Team {teamId} not found.");

        if (team.Status != TeamStatus.Proposed)
            throw new InvalidOperationException($"Only a Proposed team can be confirmed (current: {team.Status}).");

        team.Status = TeamStatus.Confirmed;

        var project = await db.Projects.FindAsync(team.ProjectId);
        if (project is { Status: ProjectStatus.Open })
            project.Status = ProjectStatus.TeamConfirmed;

        await db.SaveChangesAsync();

        return await BuildTeamDto(team);
    }

    /// <summary>
    /// Rejects a proposed team.
    /// </summary>
    /// <param name="teamId">Team ID.</param>
    /// <returns>The rejected team.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the team does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the team is not in Proposed status.</exception>
    public async Task<TeamDto> RejectAsync(Guid teamId)
    {
        var team = await db.Teams.FindAsync(teamId)
            ?? throw new KeyNotFoundException($"Team {teamId} not found.");

        if (team.Status != TeamStatus.Proposed)
            throw new InvalidOperationException($"Only a Proposed team can be rejected (current: {team.Status}).");

        team.Status = TeamStatus.Rejected;
        await db.SaveChangesAsync();

        return await BuildTeamDto(team);
    }

    /// <summary>
    /// Returns all team memberships for a user.
    /// </summary>
    /// <param name="userId">Application user ID.</param>
    /// <returns>The user's team memberships with project context.</returns>
    public async Task<List<UserTeamMembershipDto>> GetUserMembershipsAsync(Guid userId)
    {
        return await db.TeamMemberships
            .Where(tm => tm.UserId == userId)
            .Include(tm => tm.Team)
            .ThenInclude(t => t!.Project)
            .Select(tm => new UserTeamMembershipDto(
                tm.TeamId,
                tm.Team!.ProjectId,
                tm.Team.Project!.Title,
                tm.Team.Project.Status,
                tm.Team.Status,
                tm.ProjectRole,
                tm.MembershipStatus
            ))
            .ToListAsync();
    }

    private async Task<TeamDto> BuildTeamDto(TeamEntity team)
    {
        var members = await db.TeamMemberships
            .Where(tm => tm.TeamId == team.Id)
            .Include(tm => tm.User)
            .Select(tm => new TeamMembership(
                tm.Id,
                new UserDto(tm.User!.Id, tm.User.DisplayName, tm.User.Email, tm.User.Role),
                tm.ProjectRole,
                tm.MembershipStatus))
            .ToListAsync();

        return new TeamDto(team.Id, team.Source, team.Status, team.CreatedAt, members);
    }
}
