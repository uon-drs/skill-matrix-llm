namespace SkillMatrixLlm.LlmWorker.Config;

public class WorkerOptions
{
  public string StorageConnectionString { get; set; } = string.Empty;
  public string InputQueueName { get; set; } = "project-descriptions";
  public string OutputQueuename { get; set; } = "skill-requirements";
  public string PoisonQueueName { get; set; } = "project-descriptions-poison";
  public int MaxDequeueCount { get; set; } = 5;
  public string AnthropicApiKey { get; set; } = string.Empty;
  public string KeyVaultName { get; set; } = string.Empty;
}
