using Invop.Saga.Orchestrator.Core.Outbox;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Invop.Saga.Orchestrator.Core.Tests.Outbox;

public sealed class OutboxProcessorTests
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly OutboxProcessorOptions _options;
    private readonly OutboxProcessor _sut;

    public OutboxProcessorTests()
    {
        _outboxRepository = Substitute.For<IOutboxRepository>();
        _logger = Substitute.For<ILogger<OutboxProcessor>>();
        _options = new OutboxProcessorOptions
        {
            MaxRetryAttempts = 3,
            BatchSize = 100,
            PublishedMessageTtlSeconds = 3600,
            RetryBaseDelay = TimeSpan.FromMilliseconds(10), // Short delay for tests
            UseExponentialBackoff = false // Disable for deterministic tests
        };
        _sut = new OutboxProcessor(_outboxRepository, _logger, _options);
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenRepositoryIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new OutboxProcessor(null!, _logger, _options));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new OutboxProcessor(_outboxRepository, null!, _options));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenOptionsIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new OutboxProcessor(_outboxRepository, _logger, null!));
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WithNoPendingMessages_CompletesSuccessfully()
    {
        // Arrange
        _outboxRepository.GetPendingOutboxMessagesAsync(Arg.Any<CancellationToken>())
            .Returns(Array.Empty<OutboxMessageWrapper>());

        // Act
        await _sut.ProcessPendingMessagesAsync();

        // Assert
        await _outboxRepository.Received(1).GetPendingOutboxMessagesAsync(Arg.Any<CancellationToken>());
        await _outboxRepository.DidNotReceive().MarkAsProcessingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _outboxRepository.DidNotReceive().MarkAsPublishedAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WithPendingMessages_MarksMessagesAsProcessingThenPublished()
    {
        // Arrange
        var message = CreateTestOutboxMessage("test-key-1");
        _outboxRepository.GetPendingOutboxMessagesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { message });

        // Act
        await _sut.ProcessPendingMessagesAsync();

        // Assert
        Received.InOrder(() =>
        {
            _outboxRepository.GetPendingOutboxMessagesAsync(Arg.Any<CancellationToken>());
            _outboxRepository.MarkAsProcessingAsync(message.IdempotencyKey, Arg.Any<CancellationToken>());
            _outboxRepository.MarkAsPublishedAsync(
                message.IdempotencyKey,
                _options.PublishedMessageTtlSeconds,
                Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WithMultiplePendingMessages_ProcessesAllMessages()
    {
        // Arrange
        var messages = new[]
        {
            CreateTestOutboxMessage("key-1"),
            CreateTestOutboxMessage("key-2"),
            CreateTestOutboxMessage("key-3")
        };
        _outboxRepository.GetPendingOutboxMessagesAsync(Arg.Any<CancellationToken>())
            .Returns(messages);

        // Act
        await _sut.ProcessPendingMessagesAsync();

        // Assert
        await _outboxRepository.Received(3).MarkAsProcessingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _outboxRepository.Received(3).MarkAsPublishedAsync(
            Arg.Any<string>(),
            _options.PublishedMessageTtlSeconds,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WithTransientFailure_RetriesWithPolly()
    {
        // Arrange
        var message = CreateTestOutboxMessage("failing-key");
        _outboxRepository.GetPendingOutboxMessagesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { message });

        var callCount = 0;
        _outboxRepository.MarkAsProcessingAsync(message.IdempotencyKey, Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callCount++;
                if (callCount <= 2)
                {
                    throw new InvalidOperationException("Transient failure");
                }

                return ValueTask.CompletedTask;
            });

        // Act
        await _sut.ProcessPendingMessagesAsync();

        // Assert - Polly should retry until success
        callCount.ShouldBe(3);
        await _outboxRepository.Received(3).MarkAsProcessingAsync(message.IdempotencyKey, Arg.Any<CancellationToken>());
        await _outboxRepository.Received(1).MarkAsPublishedAsync(
            message.IdempotencyKey,
            _options.PublishedMessageTtlSeconds,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WithPermanentFailure_ExhaustsRetries()
    {
        // Arrange
        var message = CreateTestOutboxMessage("permanently-failing-key");
        _outboxRepository.GetPendingOutboxMessagesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { message });

        _outboxRepository.MarkAsProcessingAsync(message.IdempotencyKey, Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException("Permanent failure"));

        // Act
        await _sut.ProcessPendingMessagesAsync();

        // Assert - Should retry MaxRetryAttempts times, then mark as failed
        await _outboxRepository.Received(_options.MaxRetryAttempts + 1).MarkAsProcessingAsync(
            message.IdempotencyKey,
            Arg.Any<CancellationToken>());

        await _outboxRepository.Received().MarkAsFailedAsync(
            message.IdempotencyKey,
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());

        await _outboxRepository.DidNotReceive().MarkAsPublishedAsync(
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WithMessageExceedingMaxRetries_SkipsMessage()
    {
        // Arrange
        var message = CreateTestOutboxMessage("exceeded-key", attemptCount: 3);
        _outboxRepository.GetPendingOutboxMessagesAsync(Arg.Any<CancellationToken>())
            .Returns(new[] { message });

        // Act
        await _sut.ProcessPendingMessagesAsync();

        // Assert
        await _outboxRepository.DidNotReceive().MarkAsProcessingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _outboxRepository.DidNotReceive().MarkAsPublishedAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WithBatchSizeLimit_ProcessesOnlyBatchSizeMessages()
    {
        // Arrange
        _options.BatchSize = 2;
        var messages = new[]
        {
            CreateTestOutboxMessage("key-1"),
            CreateTestOutboxMessage("key-2"),
            CreateTestOutboxMessage("key-3"),
            CreateTestOutboxMessage("key-4")
        };
        _outboxRepository.GetPendingOutboxMessagesAsync(Arg.Any<CancellationToken>())
            .Returns(messages);

        // Act
        await _sut.ProcessPendingMessagesAsync();

        // Assert
        await _outboxRepository.Received(2).MarkAsProcessingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        await _outboxRepository.Received(2).MarkAsPublishedAsync(
            Arg.Any<string>(),
            Arg.Any<int>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WithCancellation_StopsProcessing()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var messages = new[]
        {
            CreateTestOutboxMessage("key-1"),
            CreateTestOutboxMessage("key-2"),
            CreateTestOutboxMessage("key-3")
        };
        _outboxRepository.GetPendingOutboxMessagesAsync(Arg.Any<CancellationToken>())
            .Returns(messages);

        _outboxRepository.MarkAsProcessingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(callInfo =>
            {
                cts.Cancel();
                return ValueTask.CompletedTask;
            });

        // Act
        await _sut.ProcessPendingMessagesAsync(cts.Token);

        // Assert
        await _outboxRepository.Received(1).MarkAsProcessingAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ProcessPendingMessagesAsync_WithOneFailedMessageAmongMany_ContinuesProcessingOthers()
    {
        // Arrange
        var messages = new[]
        {
            CreateTestOutboxMessage("key-1"),
            CreateTestOutboxMessage("key-2"),
            CreateTestOutboxMessage("key-3")
        };
        _outboxRepository.GetPendingOutboxMessagesAsync(Arg.Any<CancellationToken>())
            .Returns(messages);

        _outboxRepository.MarkAsProcessingAsync("key-2", Arg.Any<CancellationToken>())
            .Returns(_ => throw new InvalidOperationException("Failure on key-2"));

        // Act
        await _sut.ProcessPendingMessagesAsync();

        // Assert
        await _outboxRepository.Received(1).MarkAsPublishedAsync("key-1", Arg.Any<int>(), Arg.Any<CancellationToken>());

        // key-2 should be retried multiple times then marked as failed
        await _outboxRepository.Received(_options.MaxRetryAttempts + 1).MarkAsProcessingAsync("key-2", Arg.Any<CancellationToken>());

        await _outboxRepository.Received(1).MarkAsPublishedAsync("key-3", Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    private static OutboxMessageWrapper CreateTestOutboxMessage(
        string idempotencyKey,
        int attemptCount = 0)
    {
        return new OutboxMessageWrapper
        {
            IdempotencyKey = idempotencyKey,
            StepName = "TestStep",
            CorrelationId = Guid.NewGuid().ToString(),
            MessageType = "TestMessage",
            Payload = "test-payload"u8.ToArray(),
            SenderId = "test-sender",
            CreatedAt = DateTime.UtcNow,
            State = OutboxMessageState.Pending,
            AttemptCount = attemptCount
        };
    }
}
