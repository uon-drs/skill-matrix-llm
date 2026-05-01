namespace SkillMatrixLlm.Api.Services;

using System.Security.Claims;
using Data;
using Enums;
using Microsoft.EntityFrameworkCore;
using Models.User;
using UserEntity = SkillMatrixLlm.Api.Data.Entities.User;

/// <summary>Manages application-level user records in the local database.</summary>
public class AppUserService(AppDbContext db)
{
  /// <summary>
  /// Creates or updates the calling user's database record from their Keycloak JWT claims.
  /// </summary>
  /// <param name="principal">The authenticated principal whose claims are used.</param>
  /// <returns>The user's full profile after the upsert.</returns>
  /// <exception cref="InvalidOperationException">Thrown when required claims are missing.</exception>
  public async Task<UserProfileDto> SyncFromClaims(ClaimsPrincipal principal)
  {
    var keycloakId = principal.FindFirstValue("sub")
        ?? throw new InvalidOperationException("Missing 'sub' claim.");
    var displayName = principal.FindFirstValue(System.Security.Claims.ClaimTypes.Name)
        ?? principal.FindFirstValue("preferred_username")
        ?? throw new InvalidOperationException("Missing name claim.");
    var email = principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email)
        ?? throw new InvalidOperationException("Missing email claim.");

    var user = await db.Users.FirstOrDefaultAsync(u => u.KeycloakId == keycloakId);

    if (user is null)
    {
      user = new UserEntity { KeycloakId = keycloakId, DisplayName = displayName, Email = email };
      db.Users.Add(user);
    }
    else if (user.DisplayName != displayName || user.Email != email)
    {
      user.DisplayName = displayName;
      user.Email = email;
    }

    await db.SaveChangesAsync();

    return await BuildProfile(user.Id);
  }

  /// <summary>
  /// Returns a user's full profile including their skill proficiency records.
  /// </summary>
  /// <param name="id">The application user ID.</param>
  /// <returns>The user's profile.</returns>
  /// <exception cref="KeyNotFoundException">Thrown when no user with that ID exists.</exception>
  public async Task<UserProfileDto> GetProfile(Guid id)
  {
    var exists = await db.Users.AnyAsync(u => u.Id == id);
    if (!exists)
    {
      throw new KeyNotFoundException($"User {id} not found.");
    }

    return await BuildProfile(id);
  }

  /// <summary>
  /// Returns a user's full profile looked up by their Keycloak subject ID.
  /// </summary>
  /// <param name="keycloakId">The Keycloak subject claim value.</param>
  /// <returns>The user's profile.</returns>
  /// <exception cref="KeyNotFoundException">Thrown when no matching user record exists.</exception>
  public async Task<UserProfileDto> GetProfileByKeycloakId(string keycloakId)
  {
    var user = await db.Users.FirstOrDefaultAsync(u => u.KeycloakId == keycloakId)
        ?? throw new KeyNotFoundException($"No application record found for the current user. Call POST /api/users/login first.");

    return await BuildProfile(user.Id);
  }

  /// <summary>
  /// Lists application users with optional filters.
  /// </summary>
  /// <param name="name">Case-insensitive substring match on display name.</param>
  /// <param name="skillId">Only include users who have a proficiency record for this skill.</param>
  /// <param name="role">Only include users with this application role.</param>
  /// <returns>Matching users.</returns>
  public async Task<List<UserSummaryDto>> ListUsers(string? name, Guid? skillId, Role? role)
  {
    var query = db.Users.AsQueryable();

    if (!string.IsNullOrWhiteSpace(name))
    {
      var lower = name.ToLower();
      query = query.Where(u => u.DisplayName.ToLower().Contains(lower));
    }

    if (skillId.HasValue)
    {
      query = query.Where(u => db.UserSkills.Any(us => us.UserId == u.Id && us.SkillId == skillId.Value));
    }

    if (role.HasValue)
    {
      query = query.Where(u => u.Role == role.Value);
    }

    return await query
        .Select(u => new UserSummaryDto(u.Id, u.DisplayName, u.Email, u.Role))
        .ToListAsync();
  }

  /// <summary>
  /// Updates a user's application role.
  /// </summary>
  /// <param name="userId">The application user ID.</param>
  /// <param name="role">The role to assign.</param>
  /// <exception cref="KeyNotFoundException">Thrown when no user with that ID exists.</exception>
  public async Task SetRole(Guid userId, Role role)
  {
    var user = await db.Users.FindAsync(userId)
        ?? throw new KeyNotFoundException($"User {userId} not found.");

    user.Role = role;
    await db.SaveChangesAsync();
  }

  private async Task<UserProfileDto> BuildProfile(Guid userId)
  {
    var user = await db.Users.FindAsync(userId)
        ?? throw new KeyNotFoundException($"User {userId} not found.");

    var skills = await db.UserSkills
        .Where(us => us.UserId == userId)
        .Include(us => us.Skill)
        .Select(us => new UserSkillDto(us.SkillId, us.Skill!.Name, us.Level))
        .ToListAsync();

    return new UserProfileDto(user.Id, user.DisplayName, user.Email, user.Role, skills);
  }
}
