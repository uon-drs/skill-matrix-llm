namespace SkillMatrixLlm.Api.Data.Entities;

using SkillMatrixLlm.Api.Enums;

/// <summary>Represents an authenticated user in the application database.</summary>
public class User
{
  /// <summary>Primary key.</summary>
  public Guid Id { get; set; }

  /// <summary>The corresponding user ID in Keycloak.</summary>
  public required string KeycloakId { get; set; }

  /// <summary>Display name shown in the UI.</summary>
  public required string DisplayName { get; set; }

  /// <summary>Email address.</summary>
  public required string Email { get; set; }

  /// <summary>Application role determining what the user can do.</summary>
  public Role Role { get; set; }
}
