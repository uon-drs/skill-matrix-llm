namespace SkillMatrixLlm.Api.Models;

/// <summary>API model representing a skill in the catalogue.</summary>
/// <param name="Id">Skill ID.</param>
/// <param name="Name">Display name of the skill.</param>
public record Skill(Guid Id, string Name);
