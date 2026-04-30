namespace SkillMatrixLlm.Api.Models.Projects;

using System.ComponentModel.DataAnnotations;

/// <summary>Request body for creating a new project.</summary>
/// <param name="Title">Short title of the project.</param>
/// <param name="Description">Full description of the project's goals and requirements.</param>
/// <param name="DesiredTeamSize">Target number of team members.</param>
/// <param name="Timeline">Expected timeline or duration of the project.</param>
public record CreateProjectRequest(
    [Required][MaxLength(256)] string Title,
    [Required] string Description,
    [Required][Range(1, int.MaxValue)] int DesiredTeamSize,
    [Required][MaxLength(256)] string Timeline
);
