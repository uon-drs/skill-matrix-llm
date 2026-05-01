namespace SkillMatrixLlm.Api.Tests.Messaging;

using System.Text.Json;
using Azure;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using NSubstitute;
using SkillMatrixLlm.Api.Messaging;

/// <summary>Unit tests for <see cref="AzureStorageQueueMessageQueue{T}"/>.</summary>
public class AzureStorageQueueMessageQueueTests
{
  private sealed record TestPayload(string Name, int Count);

  private static readonly JsonSerializerOptions JsonOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    PropertyNameCaseInsensitive = true,
  };

  // Concrete response wrapper — avoids NSubstitute thread-local tracking issues
  // that occur when configuring substitutes inside argument expressions.
  private sealed class FakeQueueMessageResponse(QueueMessage[] messages) : Response<QueueMessage[]>
  {
    public override QueueMessage[] Value => messages;
    public override Response GetRawResponse() => Substitute.For<Response>();
  }

  private static QueueMessage MakeQueueMessage(string json) =>
    QueuesModelFactory.QueueMessage(
      messageId: Guid.NewGuid().ToString(),
      popReceipt: "pop-receipt",
      messageText: json,
      dequeueCount: 1);

  [Fact]
  public async Task PublishAsync_SerializesMessageAsSnakeCaseJson_AndSendsToQueue()
  {
    var client = Substitute.For<QueueClient>();
    var queue = new AzureStorageQueueMessageQueue<TestPayload>(client);
    var payload = new TestPayload("hello", 42);

    await queue.PublishAsync(payload);

    await client.Received(1).SendMessageAsync(
      Arg.Is<string>(s =>
        s.Contains("\"name\"") &&
        s.Contains("\"hello\"") &&
        s.Contains("\"count\"") &&
        s.Contains("42")),
      cancellationToken: Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task ConsumeAsync_YieldsDeserializedMessage_AndDeletesItAfterYield()
  {
    var client = Substitute.For<QueueClient>();
    var payload = new TestPayload("test", 7);
    var json = JsonSerializer.Serialize(payload, JsonOptions);
    var queueMessage = MakeQueueMessage(json);

    var withMessage = new FakeQueueMessageResponse([queueMessage]);
    client.ReceiveMessagesAsync(Arg.Any<int?>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
      .Returns(withMessage);

    var queue = new AzureStorageQueueMessageQueue<TestPayload>(client);
    // ConsumeAsync polls indefinitely — cancel once all mock messages have been received
    // so the count assertion reflects what was in the mock, not just when we stopped.
    var cts = new CancellationTokenSource();
    var consumed = new List<TestPayload>();

    await foreach (var item in queue.ConsumeAsync(cts.Token))
    {
      consumed.Add(item);
      if (consumed.Count == 1)
        cts.Cancel();
    }

    Assert.Single(consumed);
    Assert.Equal("test", consumed[0].Name);
    Assert.Equal(7, consumed[0].Count);

    await client.Received(1).DeleteMessageAsync(
      queueMessage.MessageId,
      queueMessage.PopReceipt,
      Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task ConsumeAsync_SkipsMalformedMessages_ButStillDeletesThem()
  {
    var client = Substitute.For<QueueClient>();
    var badMessage = MakeQueueMessage("not valid json {{{{");
    var goodMessage1 = MakeQueueMessage(JsonSerializer.Serialize(new TestPayload("first", 1), JsonOptions));
    var goodMessage2 = MakeQueueMessage(JsonSerializer.Serialize(new TestPayload("second", 2), JsonOptions));

    // One batch: 1 malformed + 2 valid. Using 2 valid messages makes the count assertion
    // meaningful — if malformed messages accidentally suppressed further yields we'd get 1, not 2.
    var withAll = new FakeQueueMessageResponse([badMessage, goodMessage1, goodMessage2]);
    client.ReceiveMessagesAsync(Arg.Any<int?>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>())
      .Returns(withAll);

    var queue = new AzureStorageQueueMessageQueue<TestPayload>(client);
    var cts = new CancellationTokenSource();
    var consumed = new List<TestPayload>();

    await foreach (var item in queue.ConsumeAsync(cts.Token))
    {
      consumed.Add(item);
      if (consumed.Count == 2)
        cts.Cancel();
    }

    Assert.Equal(2, consumed.Count);
    Assert.Equal("first", consumed[0].Name);
    Assert.Equal("second", consumed[1].Name);

    // All three messages deleted: malformed one without yielding, both good ones after yielding
    await client.Received(1).DeleteMessageAsync(
      badMessage.MessageId, badMessage.PopReceipt, Arg.Any<CancellationToken>());
    await client.Received(1).DeleteMessageAsync(
      goodMessage1.MessageId, goodMessage1.PopReceipt, Arg.Any<CancellationToken>());
    await client.Received(1).DeleteMessageAsync(
      goodMessage2.MessageId, goodMessage2.PopReceipt, Arg.Any<CancellationToken>());
  }

  [Fact]
  public async Task ConsumeAsync_ReturnsEmptySequence_WhenCancelledBeforeFirstPoll()
  {
    var client = Substitute.For<QueueClient>();
    var queue = new AzureStorageQueueMessageQueue<TestPayload>(client);
    var cts = new CancellationTokenSource();
    cts.Cancel();

    var consumed = new List<TestPayload>();
    await foreach (var item in queue.ConsumeAsync(cts.Token))
    {
      consumed.Add(item);
    }

    Assert.Empty(consumed);
    await client.DidNotReceive().ReceiveMessagesAsync(
      Arg.Any<int?>(), Arg.Any<TimeSpan?>(), Arg.Any<CancellationToken>());
  }
}
