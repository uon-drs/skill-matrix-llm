namespace SkillMatrixLlm.LlmWorker.Config;

/// <summary>Configuration options for the LLM worker, bound from the "Worker" config section.</summary>
public class WorkerOptions
{
  /// <summary>Azure Storage Queue connection string.</summary>
  public string StorageConnectionString { get; set; } = string.Empty;

  /// <summary>Name of the queue from which project description payloads are consumed.</summary>
  public string InputQueueName { get; set; } = "project-descriptions";

  /// <summary>Name of the queue to which skill requirements results are published.</summary>
  public string OutputQueuename { get; set; } = "skill-requirements";

  /// <summary>Name of the dead-letter queue for messages that exceed <see cref="MaxDequeueCount"/>.</summary>
  public string PoisonQueueName { get; set; } = "project-descriptions-poison";

  /// <summary>Maximum number of times a message may be dequeued before it is moved to the poison queue.</summary>
  public int MaxDequeueCount { get; set; } = 5;

  /// <summary>Anthropic API key used to authenticate requests to the Claude API.</summary>
  public string AnthropicApiKey { get; set; } = string.Empty;

  /// <summary>Azure Key Vault name from which secrets are loaded at startup. Leave empty to skip Key Vault.</summary>
  public string KeyVaultName { get; set; } = string.Empty;
}
