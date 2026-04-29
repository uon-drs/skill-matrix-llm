namespace SkillMatrixLlm.Api.Tests.Fixtures;

using System.Security.Claims;
using System.Text.Json;
using Data;
using Data.Seeder;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Services.Contracts;

/// <summary>
/// Test host factory for integration tests.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
  /// <inheritdoc />
  protected override void ConfigureWebHost(IWebHostBuilder builder) =>
    builder.ConfigureServices(services =>
    {
      services
        .AddAuthentication(TestAuthHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
          TestAuthHandler.SchemeName, _ => {});

      services.RemoveAll<DbContextOptions<AppDbContext>>();
      var inMemoryProvider = new ServiceCollection()
        .AddEntityFrameworkInMemoryDatabase()
        .BuildServiceProvider();
      services.AddDbContext<AppDbContext>(o =>
        o.UseInMemoryDatabase("TestDb").UseInternalServiceProvider(inMemoryProvider));

      services.AddTransient<IEmailSender>(_ => Mock.Of<IEmailSender>());
      services.AddSingleton<IKeycloakDataSeeder>(_ => Mock.Of<IKeycloakDataSeeder>());
    });

  /// <summary>
  /// Creates an <see cref="HttpClient" /> that sends requests without any authentication header (anonymous).
  /// </summary>
  public HttpClient CreateAnonymousClient() => CreateClient();

  /// <summary>
  /// Creates an <see cref="HttpClient" /> whose requests are authenticated using the supplied <paramref name="claims" />.
  /// The claims are serialised into the <see cref="TestAuthHandler.ClaimsHeader" /> default request header.
  /// </summary>
  /// <param name="claims">Claims to include in the test identity.</param>
  public HttpClient CreateAuthenticatedClient(IEnumerable<Claim> claims)
  {
    var client = CreateClient();
    var json = JsonSerializer.Serialize(claims.Select(x => new
    {
      x.Type, x.Value
    }));
    client.DefaultRequestHeaders.Add(TestAuthHandler.ClaimsHeader, json);

    return client;
  }
}
