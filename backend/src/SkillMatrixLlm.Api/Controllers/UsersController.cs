namespace SkillMatrixLlm.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.User;
using Models.Teams;
using Auth;
using Enums;
using Services;
using System.Net.Mime;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(KeycloakUserService keycloakUser, AppUserService appUser, UserSkillService userSkill, TeamService teamService) : ControllerBase
{
    // -------------------------------------------------------------------------
    // Keycloak user management (admin operations against Keycloak directly)
    // -------------------------------------------------------------------------

    /// <summary>Gets a Keycloak user by id.</summary>
    /// <param name="id">Keycloak user id.</param>
    /// <returns>User model.</returns>
    [HttpGet("{id}")]
    [Authorize(nameof(AuthPolicies.CanViewUsers))]
    [ProducesResponseType(typeof(User), 200)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> Get(string id)
    {
        try
        {
            return Ok(await keycloakUser.Get(id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>Lists groups (roles) from Keycloak.</summary>
    /// <returns>Groups.</returns>
    [HttpGet("groups")]
    [Authorize(nameof(AuthPolicies.CanViewUsers))]
    [ProducesResponseType(typeof(List<Group>), 200)]
    public async Task<ActionResult<List<Group>>> ListGroups() => Ok(await keycloakUser.ListGroups());

    /// <summary>Creates a new user in Keycloak.</summary>
    /// <param name="model">User model.</param>
    /// <returns>Creation result.</returns>
    [HttpPost]
    [Authorize(nameof(AuthPolicies.CanCreateUsers))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create(CreateUserRequest model)
    {
        try
        {
            await keycloakUser.Create(model);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Updates a Keycloak user.</summary>
    /// <param name="id">Keycloak user id.</param>
    /// <param name="model">User model.</param>
    /// <returns>Update result.</returns>
    [HttpPut("{id}")]
    [Authorize(nameof(AuthPolicies.CanUpdateUsers))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Set(string id, CreateUserRequest model)
    {
        try
        {
            await keycloakUser.Set(id, model);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Deletes a Keycloak user.</summary>
    /// <param name="id">Keycloak user id.</param>
    /// <returns>Deletion result.</returns>
    [HttpDelete("{id}")]
    [Authorize(nameof(AuthPolicies.CanDeleteUsers))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(string id)
    {
        try
        {
            await keycloakUser.Delete(id);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // -------------------------------------------------------------------------
    // Application-level user service (database-backed)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Syncs the calling user's database record from their Keycloak JWT claims,
    /// creating the record on first login or updating it when claims change.
    /// </summary>
    /// <returns>The caller's full profile.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserProfileDto>> Login()
    {
        try
        {
            return Ok(await appUser.SyncFromClaims(User));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>Returns the calling user's full profile including skill proficiencies.</summary>
    /// <returns>The caller's profile.</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileDto), 200)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserProfileDto>> GetMe()
    {
        var keycloakId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(keycloakId))
        {
            return BadRequest("Missing 'sub' claim.");
        }

        try
        {
            var profile = await appUser.GetProfileByKeycloakId(keycloakId);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>Lists all application users with optional search/filter. Requires admin access.</summary>
    /// <param name="name">Case-insensitive name filter.</param>
    /// <param name="skillId">Filter to users with a proficiency record for this skill.</param>
    /// <param name="role">Filter by application role.</param>
    /// <returns>Matching users.</returns>
    [HttpGet]
    [Authorize(nameof(AuthPolicies.CanViewUsers))]
    [ProducesResponseType(typeof(List<UserSummaryDto>), 200)]
    public async Task<ActionResult<List<UserSummaryDto>>> List(
        [FromQuery] string? name,
        [FromQuery] Guid? skillId,
        [FromQuery] Role? role)
        => Ok(await appUser.ListUsers(name, skillId, role));

    /// <summary>Updates a user's application role. Requires admin access.</summary>
    /// <param name="id">Application user ID.</param>
    /// <param name="request">Role assignment request.</param>
    /// <returns>No content on success.</returns>
    [HttpPut("{id:guid}/role")]
    [Authorize(nameof(AuthPolicies.CanUpdateUsers))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> SetRole(Guid id, SetRoleRequest request)
    {
        try
        {
            await appUser.SetRole(id, request.Role);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // -------------------------------------------------------------------------
    // User skill management — self-service + admin override
    // -------------------------------------------------------------------------

    /// <summary>Adds a skill proficiency record to a user's profile.</summary>
    /// <param name="userId">Application user ID.</param>
    /// <param name="request">Skill and proficiency level to add.</param>
    /// <returns>The created skill record.</returns>
    [HttpPost("{userId:guid}/skills")]
    [ProducesResponseType(typeof(UserSkillDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserSkillDto>> AddSkill(Guid userId, AddUserSkillRequest request)
    {
        if (!await IsSelfOrAdmin(userId)) return Forbid();

        try
        {
            var dto = await userSkill.AddSkillAsync(userId, request.SkillId, request.Level);
            return CreatedAtAction(nameof(GetMe), dto);
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

    /// <summary>Updates the proficiency level for an existing skill on a user's profile.</summary>
    /// <param name="userId">Application user ID.</param>
    /// <param name="skillId">Skill catalogue ID.</param>
    /// <param name="request">New proficiency level.</param>
    /// <returns>The updated skill record.</returns>
    [HttpPut("{userId:guid}/skills/{skillId:guid}")]
    [ProducesResponseType(typeof(UserSkillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserSkillDto>> UpdateSkillLevel(Guid userId, Guid skillId, UpdateUserSkillLevelRequest request)
    {
        if (!await IsSelfOrAdmin(userId)) return Forbid();

        try
        {
            return Ok(await userSkill.UpdateLevelAsync(userId, skillId, request.Level));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>Removes a skill proficiency record from a user's profile.</summary>
    /// <param name="userId">Application user ID.</param>
    /// <param name="skillId">Skill catalogue ID.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{userId:guid}/skills/{skillId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RemoveSkill(Guid userId, Guid skillId)
    {
        if (!await IsSelfOrAdmin(userId)) return Forbid();

        try
        {
            await userSkill.RemoveSkillAsync(userId, skillId);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>Returns a user's skill set as a formatted bullet list suitable for use in LLM prompts.</summary>
    /// <param name="userId">Application user ID.</param>
    /// <returns>Plain-text bullet list of skills and proficiency levels.</returns>
    [HttpGet("{userId:guid}/skills/llm-prompt")]
    [Produces(MediaTypeNames.Text.Plain)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<string>> GetSkillsForLlmPrompt(Guid userId)
    {
        if (!await IsSelfOrAdmin(userId)) return Forbid();

        return Ok(await userSkill.FormatForLlmPromptAsync(userId));
    }

    // -------------------------------------------------------------------------
    // Team memberships — user self-view
    // -------------------------------------------------------------------------

    /// <summary>Returns all team memberships for the calling user.</summary>
    /// <returns>The caller's team memberships with project context.</returns>
    [HttpGet("me/teams")]
    [ProducesResponseType(typeof(List<UserTeamMembershipDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<UserTeamMembershipDto>>> GetMyTeams()
    {
        var keycloakId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(keycloakId))
            return BadRequest("Missing 'sub' claim.");

        try
        {
            var caller = await appUser.GetProfileByKeycloakId(keycloakId);
            return Ok(await teamService.GetUserMembershipsAsync(caller.Id));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>Returns true when the caller is the target user or holds the UpdateUsers Keycloak role.</summary>
    private async Task<bool> IsSelfOrAdmin(Guid userId)
    {
        if (User.HasClaim(Auth.ClaimTypes.Role, Roles.UpdateUsers)) return true;

        var keycloakId = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(keycloakId)) return false;

        try
        {
            var caller = await appUser.GetProfileByKeycloakId(keycloakId);
            return caller.Id == userId;
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }
}
