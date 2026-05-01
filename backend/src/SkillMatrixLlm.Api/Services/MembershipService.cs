namespace SkillMatrixLlm.Api.Services;

using Data;
using Enums;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Emails;
using Models.User;
using Services.EmailServices;
using TeamMembershipEntity = Data.Entities.TeamMembership;

/// <summary>Handles the self-service aspect of team membership — requests, invitations, and responses.</summary>
public class MembershipService(AppDbContext db, MembershipEmailService membershipEmail)
{
  /// <summary>
  /// Records a user's self-request to join a proposed team.
  /// The team must be Proposed and its parent project must be Open.
  /// A user may not have an active membership (non-Declined) on any other team within the same project.
  /// </summary>
  /// <param name="userId">Application user ID of the requesting user.</param>
  /// <param name="teamId">Team ID.</param>
  /// <param name="projectRole">The role the user wishes to fill.</param>
  /// <returns>The created membership record.</returns>
  /// <exception cref="KeyNotFoundException">Thrown when the team or user does not exist.</exception>
  /// <exception cref="InvalidOperationException">Thrown when the team or project is not in an eligible state, or the user already has an active membership on this project.</exception>
  public async Task<TeamMembership> RequestAsync(Guid userId, Guid teamId, string projectRole)
  {
    var team = await db.Teams.Include(t => t.Project).FirstOrDefaultAsync(t => t.Id == teamId)
        ?? throw new KeyNotFoundException($"Team {teamId} not found.");

    if (team.Status != TeamStatus.Proposed)
    {
      throw new InvalidOperationException($"Membership requests can only be made for Proposed teams (current: {team.Status}).");
    }

    if (team.Project!.Status != ProjectStatus.Open)
    {
      throw new InvalidOperationException($"Membership requests can only be made on Open projects (current: {team.Project.Status}).");
    }

    var user = await db.Users.FindAsync(userId)
        ?? throw new KeyNotFoundException($"User {userId} not found.");

    await EnforceNoDuplicateOnProjectAsync(userId, team.ProjectId, null);

    var membership = new TeamMembershipEntity
    {
      TeamId = teamId,
      UserId = userId,
      ProjectRole = projectRole,
      MembershipStatus = MembershipStatus.Requested,
    };

    db.TeamMemberships.Add(membership);
    await db.SaveChangesAsync();

    return new TeamMembership(membership.Id, new UserDto(user.Id, user.DisplayName, user.Email, user.Role), projectRole, MembershipStatus.Requested);
  }

  /// <summary>
  /// Accepts an invitation or request, transitioning the membership to Accepted and notifying the project manager.
  /// </summary>
  /// <param name="userId">Application user ID of the accepting user.</param>
  /// <param name="teamId">Team ID.</param>
  /// <returns>The updated membership record.</returns>
  /// <exception cref="KeyNotFoundException">Thrown when the membership does not exist.</exception>
  /// <exception cref="InvalidOperationException">Thrown when the membership is not in Invited or Requested status.</exception>
  public async Task<TeamMembership> AcceptAsync(Guid userId, Guid teamId)
      => await RespondAsync(userId, teamId, MembershipStatus.Accepted);

  /// <summary>
  /// Declines an invitation or request, transitioning the membership to Declined and notifying the project manager.
  /// </summary>
  /// <param name="userId">Application user ID of the declining user.</param>
  /// <param name="teamId">Team ID.</param>
  /// <returns>The updated membership record.</returns>
  /// <exception cref="KeyNotFoundException">Thrown when the membership does not exist.</exception>
  /// <exception cref="InvalidOperationException">Thrown when the membership is not in Invited or Requested status.</exception>
  public async Task<TeamMembership> DeclineAsync(Guid userId, Guid teamId)
      => await RespondAsync(userId, teamId, MembershipStatus.Declined);

  private async Task<TeamMembership> RespondAsync(Guid userId, Guid teamId, MembershipStatus response)
  {
    var membership = await db.TeamMemberships
        .Include(tm => tm.User)
        .Include(tm => tm.Team)
        .ThenInclude(t => t!.Project)
        .ThenInclude(p => p!.CreatedByUser)
        .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId)
        ?? throw new KeyNotFoundException($"No membership found for user {userId} on team {teamId}.");

    if (membership.MembershipStatus is not (MembershipStatus.Invited or MembershipStatus.Requested))
    {
      throw new InvalidOperationException($"Cannot respond to a membership that is already {membership.MembershipStatus}.");
    }

    membership.MembershipStatus = response;
    await db.SaveChangesAsync();

    var pm = membership.Team!.Project!.CreatedByUser!;
    await membershipEmail.SendMembershipResponseAsync(
        new EmailAddress(pm.Email) { Name = pm.DisplayName },
        membership.User!.DisplayName,
        membership.Team.Project.Title,
        membership.ProjectRole,
        response);

    return new TeamMembership(
        membership.Id,
        new UserDto(membership.User.Id, membership.User.DisplayName, membership.User.Email, membership.User.Role),
        membership.ProjectRole,
        response);
  }

  /// <summary>
  /// Throws <see cref="InvalidOperationException"/> if the user already has a non-Declined membership
  /// on any team within the given project, optionally excluding a specific membership record.
  /// </summary>
  private async Task EnforceNoDuplicateOnProjectAsync(Guid userId, Guid projectId, Guid? excludeMembershipId)
  {
    var teamIds = await db.Teams
        .Where(t => t.ProjectId == projectId)
        .Select(t => t.Id)
        .ToListAsync();

    var duplicate = await db.TeamMemberships.AnyAsync(tm =>
        tm.UserId == userId &&
        teamIds.Contains(tm.TeamId) &&
        tm.MembershipStatus != MembershipStatus.Declined &&
        (excludeMembershipId == null || tm.Id != excludeMembershipId));

    if (duplicate)
    {
      throw new InvalidOperationException($"User {userId} already has an active membership on a team within this project.");
    }
  }
}
