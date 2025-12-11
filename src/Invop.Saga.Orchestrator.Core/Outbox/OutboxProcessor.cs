using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;

namespace Invop.Saga.Orchestrator.Core.Outbox;

/// <summary>
/// Default implementation of the outbox processor that handles pending message publication.
/// Implements reliable message delivery with Polly-based retry logic and transactional guarantees.
/// </summary>
public sealed class OutboxProcessor : IOutboxProcessor
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly ILogger<OutboxProcessor> _logger;
    private readonly OutboxProcessorOptions _options;
    private readonly ResiliencePipeline<bool> _retryPipeline;

    public OutboxProcessor(
        IOutboxRepository outboxRepository,
        ILogger<OutboxProcessor> logger,
        OutboxProcessorOptions options)
    {
        ArgumentNullException.ThrowIfNull(outboxRepository);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(options);

        _outboxRepository = outboxRepository;
        _logger = logger;
        _options = options;
        _retryPipeline = BuildRetryPipeline();
    }

    /// <summary>
    /// Processes all pending outbox messages by publishing them to the message broker.
    /// Messages are marked as published or failed based on the publishing outcome.
    /// Uses Polly retry strategy with exponential backoff for resilience.
    /// </summary>
    public async ValueTask ProcessPendingMessagesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Starting outbox message processing cycle");

            var pendingMessages = await _outboxRepository.GetPendingOutboxMessagesAsync(cancellationToken);

            var messagesList = pendingMessages.ToList();

            if (messagesList.Count is 0)
            {
                _logger.LogDebug("No pending outbox messages found");
                return;
            }

            _logger.LogInformation("Found {MessageCount} pending outbox messages to process", messagesList.Count);

            var processedCount = 0;
            var failedCount = 0;

            foreach (var message in messagesList.Take(_options.BatchSize))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Processing cancelled, stopping outbox processing");
                    break;
                }

                // Skip messages that exceeded retry limit
                if (message.AttemptCount >= _options.MaxRetryAttempts)
                {
                    _logger.LogWarning(
                        "Message {IdempotencyKey} exceeded max retry attempts ({MaxAttempts}), skipping",
                        message.IdempotencyKey,
                        _options.MaxRetryAttempts);
                    continue;
                }

                try
                {
                    var success = await _retryPipeline.ExecuteAsync(
                        async ct => await ProcessSingleMessageAsync(message, ct),
                        cancellationToken);

                    if (success)
                    {
                        processedCount++;
                    }
                    else
                    {
                        failedCount++;
                    }
                }
                catch (Exception ex)
                {
                    failedCount++;
                    _logger.LogError(
                        ex,
                        "Failed to process outbox message {IdempotencyKey} after {MaxAttempts} attempts (Step: {StepName}, CorrelationId: {CorrelationId}): {ErrorMessage}",
                        message.IdempotencyKey,
                        _options.MaxRetryAttempts,
                        message.StepName,
                        message.CorrelationId,
                        ex.Message);

                    await HandleMessageFailureAsync(message, ex.Message, cancellationToken);
                }
            }

            _logger.LogInformation(
                "Outbox processing cycle completed: {ProcessedCount} processed, {FailedCount} failed",
                processedCount,
                failedCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during outbox processing cycle: {ErrorMessage}", ex.Message);
        }
    }

    private ResiliencePipeline<bool> BuildRetryPipeline()
    {
        var retryOptions = new RetryStrategyOptions<bool>
        {
            MaxRetryAttempts = _options.MaxRetryAttempts,
            BackoffType = _options.UseExponentialBackoff ? DelayBackoffType.Exponential : DelayBackoffType.Constant,
            Delay = _options.RetryBaseDelay,
            UseJitter = true,
            ShouldHandle = new PredicateBuilder<bool>()
                .HandleResult(false)
                .Handle<Exception>(ex => ex is not OperationCanceledException),
            OnRetry = args =>
            {
                _logger.LogWarning(
                    "Retry attempt {AttemptNumber} for message processing after {Delay}ms. Outcome: {Outcome}",
                    args.AttemptNumber,
                    args.RetryDelay.TotalMilliseconds,
                    args.Outcome);

                return ValueTask.CompletedTask;
            }
        };

        return new ResiliencePipelineBuilder<bool>()
            .AddRetry(retryOptions)
            .Build();
    }

    private async ValueTask<bool> ProcessSingleMessageAsync(
        OutboxMessageWrapper message,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug(
                "Processing outbox message {IdempotencyKey} (Step: {StepName}, CorrelationId: {CorrelationId})",
                message.IdempotencyKey,
                message.StepName,
                message.CorrelationId);

            // Mark as processing to prevent concurrent processing
            await _outboxRepository.MarkAsProcessingAsync(message.IdempotencyKey, cancellationToken);

            // TODO: Publish message to message broker
            // This will be implemented when the message broker integration is added
            // For now, we'll simulate successful processing
            await PublishMessageAsync(message, cancellationToken);

            // Mark as published with TTL for auto-cleanup
            await _outboxRepository.MarkAsPublishedAsync(
                message.IdempotencyKey,
                _options.PublishedMessageTtlSeconds,
                cancellationToken);

            _logger.LogInformation(
                "Successfully published outbox message {IdempotencyKey} (Step: {StepName}, CorrelationId: {CorrelationId})",
                message.IdempotencyKey,
                message.StepName,
                message.CorrelationId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Transient failure processing message {IdempotencyKey}: {ErrorMessage}",
                message.IdempotencyKey,
                ex.Message);

            // Increment attempt count
            await _outboxRepository.MarkAsFailedAsync(
                message.IdempotencyKey,
                ex.Message,
                cancellationToken);

            // Return false to trigger Polly retry
            return false;
        }
    }

    private async ValueTask HandleMessageFailureAsync(
        OutboxMessageWrapper message,
        string errorMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            await _outboxRepository.MarkAsFailedAsync(
                message.IdempotencyKey,
                errorMessage,
                cancellationToken);

            _logger.LogError(
                "Message {IdempotencyKey} permanently failed after {MaxAttempts} attempts",
                message.IdempotencyKey,
                _options.MaxRetryAttempts);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to mark message {IdempotencyKey} as failed: {ErrorMessage}",
                message.IdempotencyKey,
                ex.Message);
        }
    }

    private ValueTask PublishMessageAsync(OutboxMessageWrapper message, CancellationToken cancellationToken)
    {
        // TODO: Integrate with actual message broker (RabbitMQ, Azure Service Bus, etc.)
        // This is a placeholder for the actual publishing logic
        // The implementation will depend on the chosen message broker

        _logger.LogDebug(
            "Publishing message {IdempotencyKey} to message broker (MessageType: {MessageType})",
            message.IdempotencyKey,
            message.MessageType);

        // Simulate async work
        return ValueTask.CompletedTask;
    }
}
