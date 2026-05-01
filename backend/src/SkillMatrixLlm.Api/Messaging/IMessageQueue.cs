namespace SkillMatrixLlm.Api.Messaging;

/// <summary>Abstracts a message queue for publishing and consuming typed messages.</summary>
/// <typeparam name="T">The message payload type. Must be a reference type.</typeparam>
public interface IMessageQueue<T> where T : class
{
    /// <summary>Publishes a message to the queue.</summary>
    /// <param name="message">The message to publish.</param>
    /// <param name="ct">Cancellation token.</param>
    Task PublishAsync(T message, CancellationToken ct = default);

    /// <summary>
    /// Consumes messages from the queue as an async stream.
    /// Each yielded message has been received and will be deleted from the queue
    /// before the next message is produced. Runs until the cancellation token is cancelled.
    /// </summary>
    /// <param name="ct">Cancellation token used to stop consumption.</param>
    IAsyncEnumerable<T> ConsumeAsync(CancellationToken ct = default);
}
