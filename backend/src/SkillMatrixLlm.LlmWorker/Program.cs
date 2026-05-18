using Anthropic;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Storage.Queues;
using Microsoft.Extensions.Options;
using SkillMatrixLlm.LlmWorker;
using SkillMatrixLlm.LlmWorker.Config;
using SkillMatrixLlm.LlmWorker.Services;

var builder = Host.CreateApplicationBuilder(args);

// Azure Key Vault — loaded early so secrets are available to all subsequent config bindings.
var keyVaultName = builder.Configuration["Worker:KeyVaultName"];
if (!string.IsNullOrEmpty(keyVaultName))
{
  builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{keyVaultName}.vault.azure.net/"),
    new DefaultAzureCredential());
}

builder.Services.Configure<WorkerOptions>(builder.Configuration.GetSection("Worker"));

// Anthropic Claude client
builder.Services.AddSingleton(sp =>
{
  var opts = sp.GetRequiredService<IOptions<WorkerOptions>>().Value;
  return new AnthropicClient { ApiKey = opts.AnthropicApiKey };
});
builder.Services.AddSingleton(sp => sp.GetRequiredService<AnthropicClient>().Messages);
builder.Services.AddSingleton<ILlmAnalysisService, ClaudeAnalysisService>();

// Azure Storage Queue clients — keyed so Worker can distinguish them by role.
builder.Services.AddKeyedSingleton<QueueClient>("input", (sp, _) =>
{
  var opts = sp.GetRequiredService<IOptions<WorkerOptions>>().Value;
  return new QueueClient(
    opts.StorageConnectionString,
    opts.InputQueueName,
    new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
});

builder.Services.AddKeyedSingleton<QueueClient>("output", (sp, _) =>
{
  var opts = sp.GetRequiredService<IOptions<WorkerOptions>>().Value;
  return new QueueClient(
    opts.StorageConnectionString,
    opts.OutputQueuename,
    new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
});

builder.Services.AddKeyedSingleton<QueueClient>("poison", (sp, _) =>
{
  var opts = sp.GetRequiredService<IOptions<WorkerOptions>>().Value;
  return new QueueClient(
    opts.StorageConnectionString,
    opts.PoisonQueueName,
    new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
});

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
