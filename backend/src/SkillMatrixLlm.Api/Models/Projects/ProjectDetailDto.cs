namespace SkillMatrixLlm.Api.Models.Projects;

using SkillMatrixLlm.Api.Enums;
using SkillMatrixLlm.Api.Models.User;

/// <summary>Full project detail including associated teams and recommendations.</summary>
/// <param name="Id">Project ID.</param>
/// <param name="Title">Short title of the project.</param>
/// <param name="Description">Full description of the project's goals and requirements.</param>
/// <param name="DesiredTeamSize">Target number of team members.</param>
/// <param name="Timeline">Expected timeline or duration of the project.</param>
/// <param name="Status">Current lifecycle status.</param>
/// <param name="CreatedByUser">The user who created the project.</param>
/// <param name="CreatedAt">When the project was created.</param>
/// <param name="Teams">Teams assembled for this project.</param>
/// <param name="Recommendations">LLM recommendation audit records for this project.</param>
public record ProjectDetailDto(
    Guid Id,
    string Title,
    string Description,
    int DesiredTeamSize,
    string Timeline,
    ProjectStatus Status,
    UserDto CreatedByUser,
    DateTime CreatedAt,
    List<TeamDto> Teams,
    List<Recommendation> Recommendations
);
