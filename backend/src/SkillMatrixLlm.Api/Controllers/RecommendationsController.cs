namespace SkillMatrixLlm.Api.Controllers;

using Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Services;

/// <summary>Triggers LLM-driven team recommendations for projects.</summary>
[ApiController]
[Route("api/projects/{projectId:guid}/recommendations")]
[Authorize(nameof(AuthPolicies.CanManageProjects))]
public class RecommendationsController(RecommendationService recommendations) : ControllerBase
{
  /// <summary>
  /// Dispatches a team recommendation request for the specified project to the LLM service.
  /// The project must be in Draft or Open status.
  /// </summary>
  /// <param name="projectId">Project ID.</param>
  /// <returns>Accepted when the request has been queued.</returns>
  [HttpPost]
  [ProducesResponseType(StatusCodes.Status202Accepted)]
  [ProducesResponseType(StatusCodes.Status404NotFound)]
  [ProducesResponseType(StatusCodes.Status409Conflict)]
  public async Task<ActionResult> Trigger(Guid projectId)
  {
    try
    {
      await recommendations.TriggerAsync(projectId, HttpContext.RequestAborted);
      return Accepted();
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
