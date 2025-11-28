namespace Invop.Saga.Orchestrator.Core.Outbox;

public sealed class OutboxMessageWrapper
{
    public required string StepName { get; init; }
    public required string CorrelationId { get; init; }
    public required string MessageType { get; init; }
    public required byte[] Payload { get; init; }
    public required string IdempotencyKey { get; init; }
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ProcessedOnUtc { get; init; }
    public OutboxMessageState State { get; set; } = OutboxMessageState.Pending;
    public int AttemptCount { get; set; }
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Time-to-live in seconds. Messages auto-delete after successful processing.
    /// -1 = never expire (for pending messages).
    /// Set to positive value after successful processing for cleanup.
    /// </summary>
    public int Ttl { get; set; } = -1;

    public required string SenderId { get; init; }
}
public enum OutboxMessageState
{
    /// <summary>
    /// Message is waiting to be processed.
    /// </summary>
    Pending,

    /// <summary>
    /// Message is currently being processed.
    /// </summary>
    Processing,

    /// <summary>
    /// Message was successfully published to message broker.
    /// </summary>
    Published,

    /// <summary>
    /// Message processing failed (will be retried).
    /// </summary>
    Failed
}