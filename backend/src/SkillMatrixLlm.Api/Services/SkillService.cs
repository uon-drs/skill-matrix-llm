namespace SkillMatrixLlm.Api.Services;

using Data;
using Microsoft.EntityFrameworkCore;
using SkillDto = SkillMatrixLlm.Api.Models.Skill;
using SkillEntity = SkillMatrixLlm.Api.Data.Entities.Skill;

/// <summary>Manages the curated skill catalogue.</summary>
public class SkillService(AppDbContext db)
{
  /// <summary>
  /// Returns all skills in the catalogue, optionally filtered by name.
  /// </summary>
  /// <param name="search">Case-insensitive substring match on skill name.</param>
  /// <returns>Matching skills ordered by name.</returns>
  public async Task<List<SkillDto>> ListSkills(string? search)
  {
    var query = db.Skills.AsQueryable();

    if (!string.IsNullOrWhiteSpace(search))
    {
      var lower = search.ToLower();
      query = query.Where(s => s.Name.ToLower().Contains(lower));
    }

    return await query
        .OrderBy(s => s.Name)
        .Select(s => new SkillDto(s.Id, s.Name))
        .ToListAsync();
  }

  /// <summary>
  /// Adds a new skill to the catalogue.
  /// </summary>
  /// <param name="name">Name for the new skill.</param>
  /// <returns>The created skill.</returns>
  /// <exception cref="InvalidOperationException">Thrown when a skill with that name already exists.</exception>
  public async Task<SkillDto> CreateSkill(string name)
  {
    var lower = name.ToLower();
    var exists = await db.Skills.AnyAsync(s => s.Name.ToLower() == lower);
    if (exists)
    {
      throw new InvalidOperationException($"A skill named '{name}' already exists.");
    }

    var entity = new SkillEntity { Name = name };
    db.Skills.Add(entity);
    await db.SaveChangesAsync();

    return new SkillDto(entity.Id, entity.Name);
  }

  /// <summary>
  /// Renames an existing skill.
  /// </summary>
  /// <param name="id">Skill ID.</param>
  /// <param name="name">New name for the skill.</param>
  /// <returns>The updated skill.</returns>
  /// <exception cref="KeyNotFoundException">Thrown when no skill with that ID exists.</exception>
  /// <exception cref="InvalidOperationException">Thrown when a skill with the new name already exists.</exception>
  public async Task<SkillDto> RenameSkill(Guid id, string name)
  {
    var skill = await db.Skills.FindAsync(id)
        ?? throw new KeyNotFoundException($"Skill {id} not found.");

    var lower = name.ToLower();
    var conflict = await db.Skills.AnyAsync(s => s.Id != id && s.Name.ToLower() == lower);
    if (conflict)
    {
      throw new InvalidOperationException($"A skill named '{name}' already exists.");
    }

    skill.Name = name;
    await db.SaveChangesAsync();

    return new SkillDto(skill.Id, skill.Name);
  }

  /// <summary>
  /// Removes a skill from the catalogue.
  /// </summary>
  /// <param name="id">Skill ID.</param>
  /// <exception cref="KeyNotFoundException">Thrown when no skill with that ID exists.</exception>
  /// <exception cref="InvalidOperationException">Thrown when the skill is currently assigned to one or more users.</exception>
  public async Task DeleteSkill(Guid id)
  {
    var skill = await db.Skills.FindAsync(id)
        ?? throw new KeyNotFoundException($"Skill {id} not found.");

    var inUse = await db.UserSkills.AnyAsync(us => us.SkillId == id);
    if (inUse)
    {
      throw new InvalidOperationException($"Skill '{skill.Name}' is assigned to one or more users and cannot be deleted.");
    }

    db.Skills.Remove(skill);
    await db.SaveChangesAsync();
  }
}
