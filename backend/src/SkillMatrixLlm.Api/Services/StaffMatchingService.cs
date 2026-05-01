namespace SkillMatrixLlm.Api.Services;

using System.Globalization;
using Data;
using Microsoft.EntityFrameworkCore;
using Models.Recommendations;

/// <summary>Matches users to LLM-proposed roles based on their recorded skill profiles.</summary>
public class StaffMatchingService(AppDbContext db)
{
  /// <summary>
  /// Greedily assigns the best available candidate to each role in order.
  /// Candidates are ranked by the number of required skills they hold; ties
  /// are broken by total skill count. Each user is assigned at most one role.
  /// Roles for which no candidate remains are omitted from the result.
  /// Skill name comparison is case-insensitive.
  /// </summary>
  /// <param name="roles">Roles and the skills required to fill each one.</param>
  /// <param name="ct">Cancellation token.</param>
  /// <returns>Ordered list of user-to-role assignments.</returns>
  public async Task<List<RoleAssignment>> MatchAsync(List<RoleRequirement> roles, CancellationToken ct = default)
  {
    var rawSkills = await db.UserSkills
      .Include(us => us.Skill)
      .Select(us => new { us.UserId, SkillName = us.Skill!.Name })
      .ToListAsync(ct);

    var skillsByUser = rawSkills
      .GroupBy(us => us.UserId)
      .ToDictionary(
        g => g.Key,
        g => g.Select(x => x.SkillName.ToLower(CultureInfo.InvariantCulture)).ToHashSet());

    var unassigned = new HashSet<Guid>(skillsByUser.Keys);
    var assignments = new List<RoleAssignment>();

    foreach (var role in roles)
    {
      var required = role.RequiredSkills
        .Select(s => s.ToLower(CultureInfo.InvariantCulture))
        .ToHashSet();

      var best = unassigned
        .Select(userId => new
        {
          UserId = userId,
          Score = skillsByUser[userId].Count(s => required.Contains(s)),
          Total = skillsByUser[userId].Count,
        })
        .OrderByDescending(x => x.Score)
        .ThenByDescending(x => x.Total)
        .FirstOrDefault();

      if (best is null)
      {
        break;
      }

      assignments.Add(new RoleAssignment(best.UserId, role.RoleName));
      unassigned.Remove(best.UserId);
    }

    return assignments;
  }
}
