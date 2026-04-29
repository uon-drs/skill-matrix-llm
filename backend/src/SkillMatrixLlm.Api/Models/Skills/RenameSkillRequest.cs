namespace SkillMatrixLlm.Api.Models.Skills;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Request body for renaming a skill in the catalogue.
/// </summary>
/// <param name="Name">New display name for the skill.</param>
public record RenameSkillRequest(
  [Required(ErrorMessage = "Name is required.")]
  [MaxLength(256, ErrorMessage = "Name must be 256 characters or fewer.")]
  string Name
);
