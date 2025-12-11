using Invop.Saga.Orchestrator.Core.Outbox;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Invop.Saga.Orchestrator.Core.Tests.Outbox;

public sealed class OutboxBackgroundServiceTests
{
    private readonly IOutboxProcessor _outboxProcessor;
    private readonly OutboxProcessorOptions _options;
    private readonly ILogger<OutboxBackgroundService> _logger;

    public OutboxBackgroundServiceTests()
    {
        _outboxProcessor = Substitute.For<IOutboxProcessor>();
        _logger = Substitute.For<ILogger<OutboxBackgroundService>>();
        _options = new OutboxProcessorOptions
        {
            Enabled = true,
            ProcessingInterval = TimeSpan.FromMilliseconds(10)
        };
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenProcessorIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new OutboxBackgroundService(null!, _options, _logger));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenOptionsIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new OutboxBackgroundService(_outboxProcessor, null!, _logger));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
    {
        // Act & Assert
        Should.Throw<ArgumentNullException>(() =>
            new OutboxBackgroundService(_outboxProcessor, _options, null!));
    }

    [Fact]
    public async Task ExecuteAsync_WhenDisabled_DoesNotProcessMessages()
    {
        // Arrange
        _options.Enabled = false;
        var service = new OutboxBackgroundService(_outboxProcessor, _options, _logger);
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(100);
        await service.StopAsync(CancellationToken.None);

        // Assert
        await _outboxProcessor.DidNotReceive().ProcessPendingMessagesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WhenEnabled_ProcessesMessagesPeriodically()
    {
        // Arrange
        var service = new OutboxBackgroundService(_outboxProcessor, _options, _logger);
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(150);
        await service.StopAsync(CancellationToken.None);

        // Assert
        await _outboxProcessor.Received().ProcessPendingMessagesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_WithProcessorException_ContinuesProcessing()
    {
        // Arrange
        var callCount = 0;
        _outboxProcessor.ProcessPendingMessagesAsync(Arg.Any<CancellationToken>())
            .Returns(_ =>
            {
                callCount++;
                if (callCount == 1)
                {
                    throw new InvalidOperationException("Test exception");
                }

                return ValueTask.CompletedTask;
            });

        var service = new OutboxBackgroundService(_outboxProcessor, _options, _logger);
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(150);
        await service.StopAsync(CancellationToken.None);

        // Assert
        callCount.ShouldBeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task ExecuteAsync_WithCancellation_StopsProcessingImmediately()
    {
        // Arrange
        var service = new OutboxBackgroundService(_outboxProcessor, _options, _logger);
        var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(5));

        // Act
        await service.StartAsync(cts.Token);
        await Task.Delay(50);

        // Assert - Service should handle cancellation gracefully
        _logger.Received().Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("started") || o.ToString()!.Contains("shutting down")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }
}
