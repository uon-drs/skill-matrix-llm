namespace SkillMatrixLlm.Api.Tests.Fixtures;

using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Messaging;

/// <summary>In-process message channel for use in integration tests.</summary>
public sealed class TestMessageChannel<T> : IMessageChannel<T> where T : class
{
  private readonly Channel<T> _inner = Channel.CreateUnbounded<T>();
  private int _inFlight;

  /// <summary>Enqueues a message so the consuming hosted service will pick it up.</summary>
  public void Enqueue(T message) => _inner.Writer.TryWrite(message);

  /// <summary>
  /// Waits until both the internal buffer is empty and any in-flight item
  /// has been fully processed by the consumer.
  /// </summary>
  public async Task WaitForIdleAsync(CancellationToken ct = default)
  {
    while (_inner.Reader.Count > 0 || Volatile.Read(ref _inFlight) > 0)
    {
      await Task.Delay(20, ct);
    }
  }

  /// <inheritdoc />
  public Task PublishAsync(T message, CancellationToken ct = default)
  {
    _inner.Writer.TryWrite(message);
    return Task.CompletedTask;
  }

  /// <inheritdoc />
  public async IAsyncEnumerable<T> ConsumeAsync([EnumeratorCancellation] CancellationToken ct = default)
  {
    await foreach (var item in _inner.Reader.ReadAllAsync(ct))
    {
      Interlocked.Increment(ref _inFlight);
      yield return item;
      Interlocked.Decrement(ref _inFlight);
    }
  }
}
