namespace SkillMatrixLlm.Api.Services;

using Data;
using Enums;
using Microsoft.EntityFrameworkCore;
using Models.User;
using UserSkillEntity = Data.Entities.UserSkill;

/// <summary>Manages skill proficiency records for application users.</summary>
public class UserSkillService(AppDbContext db)
{
  /// <summary>
  /// Adds a skill proficiency record to a user's profile.
  /// </summary>
  /// <param name="userId">Application user ID.</param>
  /// <param name="skillId">Skill catalogue ID.</param>
  /// <param name="level">Proficiency level.</param>
  /// <returns>The created skill record.</returns>
  /// <exception cref="KeyNotFoundException">Thrown when the user or skill does not exist.</exception>
  /// <exception cref="InvalidOperationException">Thrown when the user already has this skill assigned.</exception>
  public async Task<UserSkillDto> AddSkillAsync(Guid userId, Guid skillId, Level level)
  {
    var userExists = await db.Users.AnyAsync(u => u.Id == userId);
    if (!userExists)
    {
      throw new KeyNotFoundException($"User {userId} not found.");
    }

    var skill = await db.Skills.FindAsync(skillId)
        ?? throw new KeyNotFoundException($"Skill {skillId} not found.");

    var alreadyAssigned = await db.UserSkills.AnyAsync(us => us.UserId == userId && us.SkillId == skillId);
    if (alreadyAssigned)
    {
      throw new InvalidOperationException($"User {userId} already has skill '{skill.Name}' assigned.");
    }

    var record = new UserSkillEntity { UserId = userId, SkillId = skillId, Level = level };
    db.UserSkills.Add(record);
    await db.SaveChangesAsync();

    return new UserSkillDto(skillId, skill.Name, level);
  }

  /// <summary>
  /// Updates the proficiency level for an existing skill on a user's profile.
  /// </summary>
  /// <param name="userId">Application user ID.</param>
  /// <param name="skillId">Skill catalogue ID.</param>
  /// <param name="level">New proficiency level.</param>
  /// <returns>The updated skill record.</returns>
  /// <exception cref="KeyNotFoundException">Thrown when the user does not have this skill assigned.</exception>
  public async Task<UserSkillDto> UpdateLevelAsync(Guid userId, Guid skillId, Level level)
  {
    var record = await db.UserSkills
        .Include(us => us.Skill)
        .FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId)
        ?? throw new KeyNotFoundException($"User {userId} does not have skill {skillId} assigned.");

    record.Level = level;
    await db.SaveChangesAsync();

    return new UserSkillDto(skillId, record.Skill!.Name, level);
  }

  /// <summary>
  /// Removes a skill proficiency record from a user's profile.
  /// </summary>
  /// <param name="userId">Application user ID.</param>
  /// <param name="skillId">Skill catalogue ID.</param>
  /// <exception cref="KeyNotFoundException">Thrown when the user does not have this skill assigned.</exception>
  public async Task RemoveSkillAsync(Guid userId, Guid skillId)
  {
    var record = await db.UserSkills.FirstOrDefaultAsync(us => us.UserId == userId && us.SkillId == skillId)
        ?? throw new KeyNotFoundException($"User {userId} does not have skill {skillId} assigned.");

    db.UserSkills.Remove(record);
    await db.SaveChangesAsync();
  }

  /// <summary>
  /// Returns a user's skill set formatted as a readable bullet list for use in LLM prompts.
  /// </summary>
  /// <param name="userId">Application user ID.</param>
  /// <returns>A newline-separated bullet list of skills and proficiency levels, or an empty string if none.</returns>
  public async Task<string> FormatForLlmPromptAsync(Guid userId)
  {
    var skills = await db.UserSkills
        .Where(us => us.UserId == userId)
        .Include(us => us.Skill)
        .OrderBy(us => us.Skill!.Name)
        .Select(us => $"- {us.Skill!.Name} ({us.Level})")
        .ToListAsync();

    return string.Join("\n", skills);
  }
}
