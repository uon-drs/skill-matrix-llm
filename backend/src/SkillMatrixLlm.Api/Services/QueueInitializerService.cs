namespace SkillMatrixLlm.Api.Services;

using Azure.Storage.Queues;

/// <summary>Creates Azure Storage Queues on startup if they do not already exist.</summary>
internal sealed class QueueInitializerService(QueueClient[] clients) : IHostedService
{
  /// <inheritdoc/>
  public async Task StartAsync(CancellationToken cancellationToken)
  {
    foreach (var client in clients)
      await client.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
  }

  /// <inheritdoc/>
  public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
