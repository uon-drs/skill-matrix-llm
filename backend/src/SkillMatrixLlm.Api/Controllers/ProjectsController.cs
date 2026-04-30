namespace SkillMatrixLlm.Api.Controllers;

using Auth;
using Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Projects;
using Services;

/// <summary>Manages projects and their lifecycle.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(nameof(AuthPolicies.CanManageProjects))]
public class ProjectsController(ProjectService projects, AppUserService appUser) : ControllerBase
{
    /// <summary>Lists projects with an optional status filter.</summary>
    /// <param name="status">Optional status filter.</param>
    /// <returns>Matching projects ordered by creation date descending.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(List<Project>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Project>>> List([FromQuery] ProjectStatus? status)
        => Ok(await projects.ListAsync(status));

    /// <summary>Returns full project detail including teams and recommendations.</summary>
    /// <param name="id">Project ID.</param>
    /// <returns>Project detail.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProjectDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProjectDetailDto>> Get(Guid id)
    {
        try
        {
            return Ok(await projects.GetAsync(id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>Creates a new project in Draft status.</summary>
    /// <param name="request">Project details.</param>
    /// <returns>The created project.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Project), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Project>> Create(CreateProjectRequest request)
    {
        var keycloakId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(keycloakId))
            return BadRequest("Missing 'sub' claim.");

        try
        {
            var caller = await appUser.GetProfileByKeycloakId(keycloakId);
            var project = await projects.CreateAsync(caller.Id, request);
            return CreatedAtAction(nameof(Get), new { id = project.Id }, project);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>Updates an existing project's details. Only permitted when status is Draft or Open.</summary>
    /// <param name="id">Project ID.</param>
    /// <param name="request">Updated project details.</param>
    /// <returns>The updated project.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Project>> Update(Guid id, UpdateProjectRequest request)
    {
        try
        {
            return Ok(await projects.UpdateAsync(id, request));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>Transitions a project to a new lifecycle status.</summary>
    /// <param name="id">Project ID.</param>
    /// <param name="request">Target status.</param>
    /// <returns>The updated project.</returns>
    [HttpPut("{id:guid}/status")]
    [ProducesResponseType(typeof(Project), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<Project>> TransitionStatus(Guid id, TransitionStatusRequest request)
    {
        try
        {
            return Ok(await projects.TransitionStatusAsync(id, request.Status));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    /// <summary>Closes or cancels a project by transitioning it to Closed status.</summary>
    /// <param name="id">Project ID.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult> Close(Guid id)
    {
        try
        {
            await projects.TransitionStatusAsync(id, ProjectStatus.Closed);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }
}
