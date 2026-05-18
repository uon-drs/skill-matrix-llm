namespace SkillMatrixLlm.LlmWorker.Services;

using System.Text.Json;
using Anthropic.Models.Messages;
using Anthropic.Services;
using SkillMatrixLlm.LlmWorker.Models;

public class ClaudeAnalysisService(IMessageService messageService) : ILlmAnalysisService
{
  private static string SystemMsg() => """
  You are a technical workforce analyst. Given a project description, you identify the roles and skills required to staff the team.

  Respond ONLY with a valid JSON object — no prose, no markdown fences. The schema is:

  {"roles": [
      {
        "role_name": "string",
        "required_skills": ["string"]
      }
    ]
  }

  Rules:
  - Return exactly the number of roles that matches the requested team size.
  - Skills should be specific and technical (e.g. "ASP.NET Core", "Azure Service Bus", not "backend").
  - If a timeline is short, prefer generalist skills; if long, prefer specialists.
  - Do not include any explanation outside the JSON object.
  """;
  private static string UserMsgTemplate(ProjectDescriptionPayload p) => $"""
    Project: {p.Title}
    Description: {p.Description}
    Team size: {p.TeamSize}
    Timeline: {p.Timeline}

    Identify the roles and required skills for this team.
    """;

  private static readonly JsonSerializerOptions _jsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    PropertyNameCaseInsensitive = true,
  };

  private record LlmRolesResponse(List<RoleRequirement> Roles);


  /// <inheritdoc />
  public async Task<SkillRequirementsResult> AnalyseAsync(ProjectDescriptionPayload payload, CancellationToken ct = default) => await GetSkillRequirements(payload, ct);

  private async Task<SkillRequirementsResult> GetSkillRequirements(ProjectDescriptionPayload payload, CancellationToken ct)
  {
    MessageCreateParams createParams = new()
    {
      MaxTokens = 1024,
      Model = Model.ClaudeOpus4_6,
      Messages = [
        new() {
          Role = Role.User,
          Content = UserMsgTemplate(payload)
        }
      ],
      System = SystemMsg()
    };

    var message = await messageService.Create(createParams, ct);

    if (!message.Content.OfType<ContentBlock>().First().TryPickText(out var textBlock))
    {
      throw new InvalidOperationException("Claude returned a non-text content block.");
    }

    var rawJson = textBlock.Text;

    var parsed = JsonSerializer.Deserialize<LlmRolesResponse>(rawJson, _jsonOptions)
      ?? throw new InvalidOperationException("Claude returned null or unparseable JSON.");

    return new SkillRequirementsResult(payload.ProjectId, rawJson, parsed.Roles);
  }
}
