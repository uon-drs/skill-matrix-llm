namespace SkillMatrixLlm.Api.Controllers;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.User;
using Auth;
using Enums;
using Services;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(KeycloakUserService keycloakUser, AppUserService appUser) : ControllerBase
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
}
