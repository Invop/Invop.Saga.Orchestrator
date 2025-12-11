using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Invop.Saga.Orchestrator.Core.Outbox;

/// <summary>
/// Background service that continuously processes pending outbox messages.
/// Runs on a configured interval to ensure reliable message delivery.
/// </summary>
public sealed class OutboxBackgroundService : BackgroundService
{
    private readonly IOutboxProcessor _outboxProcessor;
    private readonly OutboxProcessorOptions _options;
    private readonly ILogger<OutboxBackgroundService> _logger;

    public OutboxBackgroundService(
        IOutboxProcessor outboxProcessor,
        OutboxProcessorOptions options,
        ILogger<OutboxBackgroundService> logger)
    {
        ArgumentNullException.ThrowIfNull(outboxProcessor);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _outboxProcessor = outboxProcessor;
        _options = options;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Outbox background service started with processing interval of {Interval}",
            _options.ProcessingInterval);

        await ProcessMessagesLoopAsync(stoppingToken);
    }

    private async Task ProcessMessagesLoopAsync(CancellationToken stoppingToken)
    {
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _outboxProcessor.ProcessPendingMessagesAsync(stoppingToken);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(
                        ex,
                        "Error occurred during outbox message processing: {ErrorMessage}",
                        ex.Message);
                }

                try
                {
                    await Task.Delay(_options.ProcessingInterval, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Expected when service is stopping
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected during shutdown
            _logger.LogInformation("Outbox background service is shutting down");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping outbox background service");
        await base.StopAsync(cancellationToken);
    }
}
