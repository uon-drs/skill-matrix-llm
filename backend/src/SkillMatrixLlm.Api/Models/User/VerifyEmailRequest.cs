namespace SkillMatrixLlm.Api.Models.User;

/// <summary>Request body for the verify-email endpoint.</summary>
/// <param name="Token">The raw verification token from the emailed link.</param>
public record VerifyEmailRequest(string Token);
