namespace SkillMatrixLlm.Api.Tests.Services;

using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using SkillMatrixLlm.Api.Config;
using SkillMatrixLlm.Api.Data;
using SkillMatrixLlm.Api.Data.Entities;
using SkillMatrixLlm.Api.Models.Emails;
using SkillMatrixLlm.Api.Services;
using SkillMatrixLlm.Api.Services.Contracts;

/// <summary>Unit tests for <see cref="EmailVerificationService"/>.</summary>
public class EmailVerificationServiceTests
{
  private static AppDbContext CreateDb() =>
    new(new DbContextOptionsBuilder<AppDbContext>()
      .UseInMemoryDatabase(Guid.NewGuid().ToString())
      .Options);

  private static IOptions<EmailVerificationOptions> DefaultOptions(int expiryMinutes = 60) =>
    Options.Create(new EmailVerificationOptions
    {
      TokenExpiryMinutes = expiryMinutes,
      FrontendBaseUrl = "http://localhost:3000"
    });

  // -------------------------------------------------------------------------
  // GenerateAndSendAsync
  // -------------------------------------------------------------------------

  [Fact]
  public async Task GenerateAndSendAsync_PersistsToken_WithCorrectExpiry()
  {
    var db = CreateDb();
    var emailSender = Substitute.For<IEmailSender>();
    var service = new EmailVerificationService(db, emailSender, DefaultOptions(expiryMinutes: 30));
    var before = DateTime.UtcNow;

    await service.GenerateAndSendAsync("user@example.com");

    var token = db.EmailVerificationTokens.Single();
    Assert.Equal("user@example.com", token.Email);
    Assert.False(string.IsNullOrEmpty(token.Token));
    Assert.Null(token.UsedAt);
    Assert.True(token.ExpiresAt >= before.AddMinutes(30));
    Assert.True(token.ExpiresAt <= DateTime.UtcNow.AddMinutes(30));
  }

  [Fact]
  public async Task GenerateAndSendAsync_SendsEmail_ToCorrectAddress()
  {
    var db = CreateDb();
    var emailSender = Substitute.For<IEmailSender>();
    var service = new EmailVerificationService(db, emailSender, DefaultOptions());

    await service.GenerateAndSendAsync("user@example.com");

    await emailSender.Received(1).SendEmail(
      Arg.Is<EmailAddress>(a => a.Address == "user@example.com"),
      "Emails/EmailVerification",
      Arg.Any<TokenEmailModel>());
  }

  [Fact]
  public async Task GenerateAndSendAsync_ActionLink_ContainsToken()
  {
    var db = CreateDb();
    TokenEmailModel? capturedModel = null;
    var emailSender = Substitute.For<IEmailSender>();
    await emailSender.SendEmail(
        Arg.Any<EmailAddress>(),
        Arg.Any<string>(),
        Arg.Do<TokenEmailModel>(m => capturedModel = m));

    var service = new EmailVerificationService(db, emailSender, DefaultOptions());
    await service.GenerateAndSendAsync("user@example.com");

    var savedToken = db.EmailVerificationTokens.Single().Token;
    Assert.NotNull(capturedModel);
    Assert.Contains(Uri.EscapeDataString(savedToken), capturedModel.ActionLink);
    Assert.StartsWith("http://localhost:3000/verify-email", capturedModel.ActionLink);
  }

  // -------------------------------------------------------------------------
  // VerifyAsync
  // -------------------------------------------------------------------------

  [Fact]
  public async Task VerifyAsync_SetsUsedAt_WhenTokenIsValid()
  {
    var db = CreateDb();
    var rawToken = "valid-token";
    db.EmailVerificationTokens.Add(new EmailVerificationToken
    {
      Id = Guid.NewGuid(),
      Email = "user@example.com",
      Token = rawToken,
      ExpiresAt = DateTime.UtcNow.AddHours(1)
    });
    await db.SaveChangesAsync();

    var service = new EmailVerificationService(db, Substitute.For<IEmailSender>(), DefaultOptions());
    await service.VerifyAsync(rawToken);

    var token = db.EmailVerificationTokens.Single();
    Assert.NotNull(token.UsedAt);
    Assert.True(token.UsedAt <= DateTime.UtcNow);
  }

  [Fact]
  public async Task VerifyAsync_ThrowsKeyNotFoundException_WhenTokenDoesNotExist()
  {
    var db = CreateDb();
    var service = new EmailVerificationService(db, Substitute.For<IEmailSender>(), DefaultOptions());

    await Assert.ThrowsAsync<KeyNotFoundException>(() => service.VerifyAsync("nonexistent-token"));
  }

  [Fact]
  public async Task VerifyAsync_ThrowsInvalidOperationException_WhenTokenIsExpired()
  {
    var db = CreateDb();
    db.EmailVerificationTokens.Add(new EmailVerificationToken
    {
      Id = Guid.NewGuid(),
      Email = "user@example.com",
      Token = "expired-token",
      ExpiresAt = DateTime.UtcNow.AddHours(-1)
    });
    await db.SaveChangesAsync();

    var service = new EmailVerificationService(db, Substitute.For<IEmailSender>(), DefaultOptions());

    await Assert.ThrowsAsync<InvalidOperationException>(() => service.VerifyAsync("expired-token"));
  }

  [Fact]
  public async Task VerifyAsync_ThrowsKeyNotFoundException_WhenTokenAlreadyUsed()
  {
    var db = CreateDb();
    db.EmailVerificationTokens.Add(new EmailVerificationToken
    {
      Id = Guid.NewGuid(),
      Email = "user@example.com",
      Token = "used-token",
      ExpiresAt = DateTime.UtcNow.AddHours(1),
      UsedAt = DateTime.UtcNow.AddMinutes(-5)
    });
    await db.SaveChangesAsync();

    var service = new EmailVerificationService(db, Substitute.For<IEmailSender>(), DefaultOptions());

    await Assert.ThrowsAsync<KeyNotFoundException>(() => service.VerifyAsync("used-token"));
  }
}
