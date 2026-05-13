# LLM Skill Analysis Worker — Implementation Plan

**Issue:** [#24 — LLM Skill Analysis Worker: Azure Queue Consumer Microservice](https://github.com/uon-drs/skill-matrix-llm/issues/24)

---

## Background

The existing system has two halves that are not yet connected:

- The **API** publishes `ProjectDescriptionPayload` to the `project-description` Azure Storage Queue when a recommendation is triggered (`RecommendationService.TriggerAsync`).
- The **API** also consumes `SkillRequirementsResult` from the `skill-requirements` queue (`TeamBuildingHostedService`) and uses those results to build team proposals.

This worker is the missing middle piece: a standalone C# microservice that reads from `project-description`, calls the Claude API, and writes structured results to `skill-requirements`.

---

## New Project

| Property | Value |
|---|---|
| Path | `backend/src/SkillMatrixLlm.LlmWorker/` |
| SDK | `Microsoft.NET.Sdk.Worker` (.NET 10, no web stack) |
| Deployment | Azure Container App (containerised via Dockerfile) |

### NuGet Packages

| Package | Version | Purpose |
|---|---|---|
| `Azure.Storage.Queues` | 12.25.0 | Queue I/O |
| `Azure.Identity` | 1.13.0 | DefaultAzureCredential (Managed Identity / dev credentials) |
| `Azure.Extensions.AspNetCore.Configuration.Secrets` | 1.3.2 | Key Vault config provider |
| `Anthropic.SDK` | latest | Claude API client |

---

## Files to Create

### `Config/WorkerOptions.cs`

```csharp
public record WorkerOptions
{
  public string StorageConnectionString { get; init; } = string.Empty;
  public string InputQueueName         { get; init; } = "project-description";
  public string OutputQueueName        { get; init; } = "skill-requirements";
  public string PoisonQueueName        { get; init; } = "project-description-poison";
  public int    MaxDequeueCount        { get; init; } = 5;
  public string AnthropicApiKey        { get; init; } = string.Empty;
  public string KeyVaultName           { get; init; } = string.Empty;
}
```

Bound from the `"Worker"` config section. Sensitive values (`StorageConnectionString`, `AnthropicApiKey`) come from Key Vault or environment variables — never hardcoded.

---

### `Models/` — Message Contracts

Duplicate the three record types from the API project. They are identical in structure; duplication keeps the two services independently deployable (the JSON wire format is the contract, not a shared assembly).

**`Models/ProjectDescriptionPayload.cs`**
```csharp
public record ProjectDescriptionPayload(
  Guid   ProjectId,
  string Title,
  string Description,
  int    TeamSize,
  string Timeline
);
```

**`Models/RoleRequirement.cs`**
```csharp
public record RoleRequirement(string RoleName, List<string> RequiredSkills);
```

**`Models/SkillRequirementsResult.cs`**
```csharp
public record SkillRequirementsResult(
  Guid              ProjectId,
  string            RawLlmResponse,
  List<RoleRequirement> Roles
);
```

---

### `Services/ILlmAnalysisService.cs`

```csharp
public interface ILlmAnalysisService
{
  Task<SkillRequirementsResult> AnalyzeAsync(
    ProjectDescriptionPayload payload,
    CancellationToken ct = default);
}
```

---

### `Services/ClaudeAnalysisService.cs`

- Implements `ILlmAnalysisService`
- Uses `Anthropic.SDK` to call model `claude-sonnet-4-6`
- **System prompt:** instructs Claude to return _only_ valid JSON — a list of `{ role_name, required_skills[] }` objects; no prose, no markdown fences
- **User prompt:** project title, description, team size, and timeline
- Stores the raw JSON string from Claude as `RawLlmResponse`
- Deserialises into `List<RoleRequirement>` (snake_case, case-insensitive)
- Throws `InvalidOperationException` on JSON parse failure — the caller (`Worker`) handles retry/poison routing

---

### `Worker.cs` (BackgroundService)

Owns all at-least-once delivery logic directly against `QueueClient` (not via `IMessageChannel<T>`, which auto-deletes before knowing if processing succeeded).

**Poll loop:**

```
loop:
  ReceiveMessages(maxMessages: 1, visibilityTimeout: 5 min)
    // 5 min >> max expected LLM call latency; message invisible to other consumers during processing

  if no messages → await Task.Delay(2 s), continue

  if msg.DequeueCount > MaxDequeueCount:
    send raw message body to poison queue
    delete from input queue
    log warning
    continue

  try:
    deserialise to ProjectDescriptionPayload
    result = await llmService.AnalyzeAsync(payload)
    serialise result, send to output queue
    delete from input queue     ← delete ONLY after success
  catch Exception:
    log error
    // do NOT delete → visibility timeout expires → message becomes visible again for retry
```

JSON options throughout: `SnakeCaseLower` + `PropertyNameCaseInsensitive = true` (matches the API's serialisation in `AzureStorageQueueMessageChannel`).

---

### `Program.cs`

```
1. Read KeyVaultName from config
2. If set → add AzureKeyVault as config source (same pattern as SkillMatrixLlm.Api/Program.cs lines 24–34)
3. Bind WorkerOptions from "Worker" section
4. Register QueueClient singletons (input, output, poison) via factory lambdas (deferred, same pattern as ServiceCollectionExtensions.cs in the API)
5. Register ClaudeAnalysisService as ILlmAnalysisService (scoped)
6. AddHostedService<Worker>()
```

---

### `appsettings.json`

```json
{
  "Worker": {
    "InputQueueName":  "project-description",
    "OutputQueueName": "skill-requirements",
    "PoisonQueueName": "project-description-poison",
    "MaxDequeueCount": 5
  }
}
```

### `appsettings.Development.json`

```json
{
  "Worker": {
    "StorageConnectionString": "UseDevelopmentStorage=true"
  }
}
```

(`AnthropicApiKey` goes in user secrets locally: `dotnet user-secrets set "Worker:AnthropicApiKey" "<key>"`)

---

### `Dockerfile`

Multi-stage build placed at `backend/src/SkillMatrixLlm.LlmWorker/Dockerfile`. Build context is the repo root so COPY paths align with the solution layout.

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY backend/src/SkillMatrixLlm.LlmWorker/SkillMatrixLlm.LlmWorker.csproj \
     backend/src/SkillMatrixLlm.LlmWorker/
RUN dotnet restore backend/src/SkillMatrixLlm.LlmWorker/SkillMatrixLlm.LlmWorker.csproj
COPY . .
RUN dotnet publish backend/src/SkillMatrixLlm.LlmWorker/SkillMatrixLlm.LlmWorker.csproj \
    -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SkillMatrixLlm.LlmWorker.dll"]
```

---

## Files to Modify

### `backend/SkillMatrixLlm.sln`

Add a project entry for `SkillMatrixLlm.LlmWorker` and (once created) the unit test project.

### `docker-compose.yml`

Add an `llm-worker` service:

```yaml
llm-worker:
  build:
    context: .
    dockerfile: backend/src/SkillMatrixLlm.LlmWorker/Dockerfile
  environment:
    - Worker__StorageConnectionString=DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;QueueEndpoint=http://azurite:10001/devstoreaccount1;
    - Worker__AnthropicApiKey=${ANTHROPIC_API_KEY}   # set in .env
  depends_on:
    - azurite
```

---

## Unit Test Project: `SkillMatrixLlm.LlmWorker.UnitTests`

**Path:** `backend/tests/SkillMatrixLlm.LlmWorker.UnitTests/`  
**Framework:** xUnit + NSubstitute (matches the existing unit test project)

| Test | Description |
|---|---|
| `AnalyzeAsync_ReturnsResult_WhenClaudeReturnsValidJson` | Mock Anthropic client returning valid JSON; assert `Roles` parsed correctly |
| `AnalyzeAsync_Throws_WhenClaudeReturnsInvalidJson` | Assert `InvalidOperationException` on malformed JSON |
| `ExecuteAsync_DeletesMessage_AfterSuccessfulProcessing` | Mock `ILlmAnalysisService` + `QueueClient`; verify delete called on success |
| `ExecuteAsync_DoesNotDeleteMessage_WhenProcessingFails` | Verify no delete on exception |
| `ExecuteAsync_MovesToPoisonQueue_WhenDequeueCountExceedsThreshold` | Verify poison-queue send + delete when `DequeueCount > MaxDequeueCount` |

---

## Verification Steps

1. `docker-compose up` — Azurite and Storage Web Explorer start.
2. Set `Worker:AnthropicApiKey` via dotnet user-secrets in the worker project.
3. `dotnet run` from `backend/src/SkillMatrixLlm.LlmWorker/` — worker starts polling.
4. Open Azure Storage Web Explorer (http://localhost:8081) and enqueue a test message on the `project-description` queue:
   ```json
   {
     "project_id": "00000000-0000-0000-0000-000000000001",
     "title": "Test Project",
     "description": "Build a REST API with authentication and a React frontend.",
     "team_size": 4,
     "timeline": "3 months"
   }
   ```
5. Observe structured logs: _message received → Claude API called → result published_.
6. Verify a `SkillRequirementsResult` message appears on the `skill-requirements` queue.
7. Start the API — `TeamBuildingHostedService` consumes the result and creates a team proposal.
8. `dotnet test` — all unit tests pass.
