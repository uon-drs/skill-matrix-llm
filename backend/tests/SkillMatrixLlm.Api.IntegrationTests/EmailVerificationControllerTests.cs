namespace SkillMatrixLlm.Api.Tests;

using System.Net;
using System.Net.Http.Json;
using Data;
using Fixtures;
using Microsoft.Extensions.DependencyInjection;
using SkillMatrixLlm.Api.Data.Entities;
using Xunit;

/// <summary>Integration tests for the email verification endpoint.</summary>
public class EmailVerificationControllerTests(ApiFactory factory) : IClassFixture<ApiFactory>, IAsyncLifetime
{
  public Task InitializeAsync()
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.EmailVerificationTokens.RemoveRange(db.EmailVerificationTokens);
    db.SaveChanges();
    return Task.CompletedTask;
  }

  public Task DisposeAsync() => Task.CompletedTask;

  // -------------------------------------------------------------------------
  // POST /api/users/verify-email
  // -------------------------------------------------------------------------

  [Fact]
  public async Task VerifyEmail_ReturnsNoContent_WithValidToken()
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.EmailVerificationTokens.Add(new EmailVerificationToken
    {
      Id = Guid.NewGuid(),
      Email = "user@example.com",
      Token = "valid-token",
      ExpiresAt = DateTime.UtcNow.AddHours(1)
    });
    await db.SaveChangesAsync();

    var response = await factory.CreateAnonymousClient()
      .PostAsJsonAsync("/api/users/verify-email", new { Token = "valid-token" });

    Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

    db.ChangeTracker.Clear();
    var token = db.EmailVerificationTokens.Single();
    Assert.NotNull(token.UsedAt);
  }

  [Fact]
  public async Task VerifyEmail_ReturnsNotFound_WithUnknownToken()
  {
    var response = await factory.CreateAnonymousClient()
      .PostAsJsonAsync("/api/users/verify-email", new { Token = "does-not-exist" });

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }

  [Fact]
  public async Task VerifyEmail_ReturnsBadRequest_WithExpiredToken()
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.EmailVerificationTokens.Add(new EmailVerificationToken
    {
      Id = Guid.NewGuid(),
      Email = "user@example.com",
      Token = "expired-token",
      ExpiresAt = DateTime.UtcNow.AddHours(-1)
    });
    await db.SaveChangesAsync();

    var response = await factory.CreateAnonymousClient()
      .PostAsJsonAsync("/api/users/verify-email", new { Token = "expired-token" });

    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
  }

  [Fact]
  public async Task VerifyEmail_ReturnsNotFound_WithAlreadyUsedToken()
  {
    using var scope = factory.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.EmailVerificationTokens.Add(new EmailVerificationToken
    {
      Id = Guid.NewGuid(),
      Email = "user@example.com",
      Token = "used-token",
      ExpiresAt = DateTime.UtcNow.AddHours(1),
      UsedAt = DateTime.UtcNow.AddMinutes(-5)
    });
    await db.SaveChangesAsync();

    var response = await factory.CreateAnonymousClient()
      .PostAsJsonAsync("/api/users/verify-email", new { Token = "used-token" });

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
  }
}
