namespace SkillMatrixLlm.Api.Data.Entities;

/// <summary>A time-limited token used to verify a user's email address.</summary>
public class EmailVerificationToken
{
  /// <summary>Primary key.</summary>
  public Guid Id { get; set; }

  /// <summary>The email address being verified.</summary>
  public string Email { get; set; } = string.Empty;

  /// <summary>The secure random token value sent in the verification link.</summary>
  public string Token { get; set; } = string.Empty;

  /// <summary>UTC time at which the token expires.</summary>
  public DateTime ExpiresAt { get; set; }

  /// <summary>UTC time at which the token was consumed. Null if not yet used.</summary>
  public DateTime? UsedAt { get; set; }
}
