namespace SkillMatrixLlm.Api.Messaging;

using System.Runtime.CompilerServices;
using System.Text.Json;
using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

/// <summary>
/// <see cref="IMessageQueue{T}"/> implementation backed by Azure Queue Storage.
/// Messages are JSON-serialised using snake_case naming and base64-encoded by the SDK.
/// </summary>
/// <typeparam name="T">The message payload type.</typeparam>
public class AzureStorageQueueMessageQueue<T>(QueueClient client) : IMessageQueue<T>
    where T : class
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(2);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true,
    };

    /// <inheritdoc/>
    public async Task PublishAsync(T message, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(message, JsonOptions);
        await client.SendMessageAsync(json, cancellationToken: ct);
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<T> ConsumeAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        while (!ct.IsCancellationRequested)
        {
            Response<QueueMessage[]> response;
            try
            {
                response = await client.ReceiveMessagesAsync(maxMessages: 32, cancellationToken: ct);
            }
            catch (OperationCanceledException)
            {
                yield break;
            }

            var messages = response.Value;

            if (messages.Length == 0)
            {
                try { await Task.Delay(PollInterval, ct); }
                catch (OperationCanceledException) { yield break; }
                continue;
            }

            foreach (var msg in messages)
            {
                if (ct.IsCancellationRequested) yield break;

                T? payload = null;
                try { payload = JsonSerializer.Deserialize<T>(msg.Body.ToString(), JsonOptions); }
                catch (JsonException) { /* malformed — delete and skip */ }

                if (payload is not null)
                    yield return payload;

                await client.DeleteMessageAsync(msg.MessageId, msg.PopReceipt, ct);
            }
        }
    }
}
