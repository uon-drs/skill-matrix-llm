namespace SkillMatrixLlm.Api.Services;

using System.Security.Cryptography;
using Config;
using Data;
using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models.Emails;
using Services.Contracts;

/// <summary>Generates and validates time-limited email verification tokens.</summary>
public class EmailVerificationService(
  AppDbContext db,
  IEmailSender emailSender,
  IOptions<EmailVerificationOptions> options)
{
  private readonly EmailVerificationOptions _options = options.Value;

  /// <summary>
  /// Creates a new verification token for <paramref name="email"/>, persists it, and
  /// sends a verification email containing a link to the frontend verify-email page.
  /// </summary>
  /// <param name="email">The email address to verify.</param>
  public async Task GenerateAndSendAsync(string email)
  {
    var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    var urlToken = Uri.EscapeDataString(rawToken);

    var entity = new EmailVerificationToken
    {
      Id = Guid.NewGuid(),
      Email = email,
      Token = rawToken,
      ExpiresAt = DateTime.UtcNow.AddMinutes(_options.TokenExpiryMinutes)
    };

    db.EmailVerificationTokens.Add(entity);
    await db.SaveChangesAsync();

    var actionLink = $"{_options.FrontendBaseUrl}/verify-email?token={urlToken}";
    var model = new TokenEmailModel(
      RecipientName: email,
      ActionLink: actionLink,
      ResendLink: string.Empty);

    await emailSender.SendEmail(
      new EmailAddress(email) { Name = email },
      "Emails/EmailVerification",
      model);
  }

  /// <summary>
  /// Marks a verification token as used, confirming email ownership.
  /// </summary>
  /// <param name="token">The raw token value from the verification link.</param>
  /// <exception cref="KeyNotFoundException">No unused token with this value exists.</exception>
  /// <exception cref="InvalidOperationException">The token has expired.</exception>
  public async Task VerifyAsync(string token)
  {
    var entity = await db.EmailVerificationTokens
      .FirstOrDefaultAsync(t => t.Token == token && t.UsedAt == null)
      ?? throw new KeyNotFoundException("Verification token not found.");

    if (entity.ExpiresAt < DateTime.UtcNow)
    {
      throw new InvalidOperationException("Verification token has expired.");
    }

    entity.UsedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();
  }
}
