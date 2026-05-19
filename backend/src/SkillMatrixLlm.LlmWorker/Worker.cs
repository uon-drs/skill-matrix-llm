namespace SkillMatrixLlm.LlmWorker;

using System.Text.Json;
using Azure.Storage.Queues;
using Microsoft.Extensions.Options;
using SkillMatrixLlm.LlmWorker.Config;
using SkillMatrixLlm.LlmWorker.Models;
using SkillMatrixLlm.LlmWorker.Services;

/// <summary>Background service that polls the input queue, invokes LLM analysis, and publishes results.</summary>
public partial class Worker(
  [FromKeyedServices("input")] QueueClient inputQueue,
  [FromKeyedServices("output")] QueueClient outputQueue,
  [FromKeyedServices("poison")] QueueClient poisonQueue,
  ILlmAnalysisService llmService,
  IOptions<WorkerOptions> workerOptions,
  ILogger<Worker> logger) : BackgroundService
{
  private static readonly TimeSpan _visibilityTimeout = TimeSpan.FromMinutes(5);
  private static readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(2);

  private static readonly JsonSerializerOptions _jsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
  };

  [LoggerMessage(Level = LogLevel.Warning, Message = "Message {MessageId} exceeded max dequeue count ({Max}); routing to poison queue.")]
  private static partial void LogPoisonRouted(ILogger logger, string messageId, long max);

  [LoggerMessage(Level = LogLevel.Information, Message = "Processed project {ProjectId}: {RoleCount} roles identified.")]
  private static partial void LogProcessed(ILogger logger, Guid projectId, int roleCount);

  [LoggerMessage(Level = LogLevel.Error, Message = "Failed to process message {MessageId}; will retry after visibility timeout.")]
  private static partial void LogProcessingFailed(ILogger logger, Exception ex, string messageId);

  /// <inheritdoc />
  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    var opts = workerOptions.Value;

    while (!stoppingToken.IsCancellationRequested)
    {
      var response = await inputQueue.ReceiveMessagesAsync(
        maxMessages: 1,
        visibilityTimeout: _visibilityTimeout,
        cancellationToken: stoppingToken);

      var messages = response.Value;

      if (messages.Length == 0)
      {
        await Task.Delay(_pollInterval, stoppingToken);
        continue;
      }

      var msg = messages[0];

      if (msg.DequeueCount > opts.MaxDequeueCount)
      {
        LogPoisonRouted(logger, msg.MessageId, msg.DequeueCount);
        _ = await poisonQueue.SendMessageAsync(msg.Body.ToString(), cancellationToken: stoppingToken);
        _ = await inputQueue.DeleteMessageAsync(msg.MessageId, msg.PopReceipt, stoppingToken);
        continue;
      }

      try
      {
        var payload = JsonSerializer.Deserialize<ProjectDescriptionPayload>(msg.Body.ToString(), _jsonOptions)
          ?? throw new InvalidOperationException("Deserialized payload was null.");

        var result = await llmService.AnalyseAsync(payload, stoppingToken);

        var resultJson = JsonSerializer.Serialize(result, _jsonOptions);
        _ = await outputQueue.SendMessageAsync(resultJson, cancellationToken: stoppingToken);

        // Delete only after successful publish to avoid data loss
        _ = await inputQueue.DeleteMessageAsync(msg.MessageId, msg.PopReceipt, stoppingToken);

        LogProcessed(logger, payload.ProjectId, result.Roles.Count);
      }
      catch (Exception ex) when (ex is not OperationCanceledException)
      {
        // Do not delete — visibility timeout expiry makes the message available for retry
        LogProcessingFailed(logger, ex, msg.MessageId);
      }
    }
  }
}
