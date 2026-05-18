namespace SkillMatrixLlm.LlmWorker.Services;

using System.Text.Json;
using Anthropic.Models.Messages;
using Anthropic.Services;
using NSubstitute;
using SkillMatrixLlm.LlmWorker.Models;

public class ClaudeAnalysisServiceTests
{
  private const string _validRolesJson = /*lang=json,strict*/ """{"roles":[{"role_name":"Backend Engineer","required_skills":["C#","ASP.NET Core"]}]}""";

  /// <summary>Builds a fake <see cref="IMessageService"/> whose <c>Create</c> returns a single text block containing <paramref name="textContent"/>.</summary>
  private static IMessageService BuildMessageService(string textContent)
  {
    var svc = Substitute.For<IMessageService>();
    svc.Create(Arg.Any<MessageCreateParams>(), Arg.Any<CancellationToken>())
      .Returns(BuildFakeMessage(textContent));
    return svc;
  }

  /// <summary>Constructs a <see cref="Message"/> from a raw dictionary, mirroring the shape the Anthropic API returns.</summary>
  private static Message BuildFakeMessage(string textContent)
  {
    var contentJson = "[{\"type\":\"text\",\"text\":" + JsonSerializer.Serialize(textContent) + "}]";
    return Message.FromRawUnchecked(new Dictionary<string, JsonElement>
    {
      ["id"] = JsonDocument.Parse("\"msg_test\"").RootElement,
      ["type"] = JsonDocument.Parse("\"message\"").RootElement,
      ["role"] = JsonDocument.Parse("\"assistant\"").RootElement,
      ["model"] = JsonDocument.Parse("\"claude-opus-4-6\"").RootElement,
      ["stop_reason"] = JsonDocument.Parse("\"end_turn\"").RootElement,
      ["stop_sequence"] = JsonDocument.Parse("null").RootElement,
      ["content"] = JsonDocument.Parse(contentJson).RootElement,
      ["usage"] = JsonDocument.Parse("{\"input_tokens\":10,\"output_tokens\":20}").RootElement,
    });
  }

  private static ProjectDescriptionPayload DefaultPayload(Guid? id = null) =>
    new(id ?? Guid.NewGuid(), "Test Project", "Build a REST API", 2, "3 months");

  [Fact]
  public async Task AnalyseAsync_ReturnsCorrectRoles_WhenApiReturnsValidJson()
  {
    var svc = new ClaudeAnalysisService(BuildMessageService(_validRolesJson));

    var result = await svc.AnalyseAsync(DefaultPayload());

    Assert.Single(result.Roles);
    Assert.Equal("Backend Engineer", result.Roles[0].RoleName);
    Assert.Contains("C#", result.Roles[0].RequiredSkills);
    Assert.Contains("ASP.NET Core", result.Roles[0].RequiredSkills);
  }

  [Fact]
  public async Task AnalyseAsync_PreservesProjectId_InResult()
  {
    var projectId = Guid.NewGuid();
    var svc = new ClaudeAnalysisService(BuildMessageService(_validRolesJson));

    var result = await svc.AnalyseAsync(DefaultPayload(projectId));

    Assert.Equal(projectId, result.ProjectId);
  }

  [Fact]
  public async Task AnalyseAsync_StoresRawLlmResponse_InResult()
  {
    var svc = new ClaudeAnalysisService(BuildMessageService(_validRolesJson));

    var result = await svc.AnalyseAsync(DefaultPayload());

    Assert.Equal(_validRolesJson, result.RawLlmResponse);
  }

  [Fact]
  public async Task AnalyseAsync_Throws_WhenJsonIsInvalid()
  {
    var svc = new ClaudeAnalysisService(BuildMessageService("not valid json {{"));

    await Assert.ThrowsAnyAsync<JsonException>(() => svc.AnalyseAsync(DefaultPayload()));
  }

  [Fact]
  public async Task AnalyseAsync_Throws_WhenApiReturnsEmptyContent()
  {
    var svc = new ClaudeAnalysisService(BuildMessageService(""));

    await Assert.ThrowsAnyAsync<JsonException>(() => svc.AnalyseAsync(DefaultPayload()));
  }

  [Fact]
  public async Task AnalyseAsync_PassesSystemPrompt_ToApi()
  {
    var messageService = Substitute.For<IMessageService>();
    messageService.Create(Arg.Any<MessageCreateParams>(), Arg.Any<CancellationToken>())
      .Returns(BuildFakeMessage(_validRolesJson));
    var svc = new ClaudeAnalysisService(messageService);

    await svc.AnalyseAsync(DefaultPayload());

    await messageService.Received(1).Create(
      Arg.Is<MessageCreateParams>(p => p.System != null && p.System.ToString().Contains("valid JSON object")),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task AnalyseAsync_BuildsUserMessage_WithPayloadFields()
  {
    var messageService = Substitute.For<IMessageService>();
    messageService.Create(Arg.Any<MessageCreateParams>(), Arg.Any<CancellationToken>())
      .Returns(BuildFakeMessage(_validRolesJson));
    var svc = new ClaudeAnalysisService(messageService);
    var payload = new ProjectDescriptionPayload(Guid.NewGuid(), "My App", "An e-commerce platform", 4, "6 months");

    await svc.AnalyseAsync(payload);

    await messageService.Received(1).Create(
      Arg.Is<MessageCreateParams>(p =>
        p.Messages[0].Content.ToString().Contains("My App") &&
        p.Messages[0].Content.ToString().Contains("An e-commerce platform") &&
        p.Messages[0].Content.ToString().Contains('4') &&
        p.Messages[0].Content.ToString().Contains("6 months")),
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task AnalyseAsync_UsesCancellationToken()
  {
    var messageService = Substitute.For<IMessageService>();
    messageService.Create(Arg.Any<MessageCreateParams>(), Arg.Any<CancellationToken>())
      .Returns(BuildFakeMessage(_validRolesJson));
    var svc = new ClaudeAnalysisService(messageService);
    var ct = new CancellationToken(canceled: false);

    await svc.AnalyseAsync(DefaultPayload(), ct);

    await messageService.Received(1).Create(Arg.Any<MessageCreateParams>(), ct);
  }
}
