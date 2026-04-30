namespace SkillMatrixLlm.Api.Models.Projects;

using System.ComponentModel.DataAnnotations;
using SkillMatrixLlm.Api.Enums;

/// <summary>Request body for transitioning a project to a new lifecycle status.</summary>
/// <param name="Status">The target status.</param>
public record TransitionStatusRequest([Required] ProjectStatus Status);
