namespace SkillMatrixLlm.Api.Models.Skills;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request body for adding a new skill to the catalogue.
/// </summary>
/// <param name="Name">Display name of the skill.</param>
public record CreateSkillRequest(
  [Required(ErrorMessage = "Name is required.")]
  [MaxLength(256, ErrorMessage = "Name must be 256 characters or fewer.")]
  string Name
);
