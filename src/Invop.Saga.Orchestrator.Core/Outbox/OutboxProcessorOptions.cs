namespace Invop.Saga.Orchestrator.Core.Outbox;

/// <summary>
/// Configuration options for the outbox message processor.
/// </summary>
public sealed class OutboxProcessorOptions
{
    /// <summary>
    /// Interval between outbox processing cycles.
    /// Default: 5 seconds.
    /// </summary>
    public TimeSpan ProcessingInterval { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Maximum number of retry attempts for failed messages.
    /// Default: 3 attempts.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay for exponential backoff retry strategy.
    /// Default: 2 seconds.
    /// </summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Whether to use exponential backoff for retry delays.
    /// When true: delays follow pattern: baseDelay * 2^attempt (with jitter).
    /// When false: constant delay equal to RetryBaseDelay.
    /// Default: true.
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;

    /// <summary>
    /// Time-to-live in seconds for successfully published messages before auto-deletion.
    /// Default: 3600 seconds (1 hour).
    /// </summary>
    public int PublishedMessageTtlSeconds { get; set; } = 3600;

    /// <summary>
    /// Maximum number of messages to process in a single batch.
    /// Default: 100 messages.
    /// </summary>
    public int BatchSize { get; set; } = 100;
}
