namespace SkillMatrixLlm.Api.Config;

/// <summary>Options for the email verification token system.</summary>
public record EmailVerificationOptions
{
  /// <summary>How long a verification token remains valid, in minutes.</summary>
  public int TokenExpiryMinutes { get; init; } = 1440;

  /// <summary>Base URL of the frontend, used to construct the verification link.</summary>
  public string FrontendBaseUrl { get; init; } = string.Empty;
}
