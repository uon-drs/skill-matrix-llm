namespace SkillMatrixLlm.Api.Controllers;

using Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models;
using Models.Skills;
using Services;

/// <summary>Manages the curated skill catalogue.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SkillsController(SkillService skills) : ControllerBase
{
  /// <summary>Lists all skills in the catalogue, optionally filtered by name.</summary>
  /// <param name="search">Case-insensitive substring filter on skill name.</param>
  /// <returns>Matching skills ordered by name.</returns>
  [HttpGet]
  [ProducesResponseType(typeof(List<Skill>), 200)]
  public async Task<ActionResult<List<Skill>>> List([FromQuery] string? search)
      => Ok(await skills.ListSkills(search));

  /// <summary>Adds a new skill to the catalogue.</summary>
  /// <param name="request">Skill creation request.</param>
  /// <returns>The created skill.</returns>
  [HttpPost]
  [Authorize(nameof(AuthPolicies.CanManageSkills))]
  [ProducesResponseType(typeof(Skill), 201)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  public async Task<ActionResult<Skill>> Create(CreateSkillRequest request)
  {
    try
    {
      var skill = await skills.CreateSkill(request.Name);
      return CreatedAtAction(nameof(List), skill);
    }
    catch (InvalidOperationException ex)
    {
      return Conflict(ex.Message);
    }
  }

  /// <summary>Renames an existing skill.</summary>
  /// <param name="id">Skill ID.</param>
  /// <param name="request">Rename request.</param>
  /// <returns>The updated skill.</returns>
  [HttpPut("{id:guid}")]
  [Authorize(nameof(AuthPolicies.CanManageSkills))]
  [ProducesResponseType(typeof(Skill), 200)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  public async Task<ActionResult<Skill>> Rename(Guid id, RenameSkillRequest request)
  {
    try
    {
      return Ok(await skills.RenameSkill(id, request.Name));
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

  /// <summary>Removes a skill from the catalogue.</summary>
  /// <param name="id">Skill ID.</param>
  /// <returns>No content on success.</returns>
  [HttpDelete("{id:guid}")]
  [Authorize(nameof(AuthPolicies.CanManageSkills))]
  [ProducesResponseType(StatusCodes.Status204NoContent)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  public async Task<ActionResult> Delete(Guid id)
  {
    try
    {
      await skills.DeleteSkill(id);
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
