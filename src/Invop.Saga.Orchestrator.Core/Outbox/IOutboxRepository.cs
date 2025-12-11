namespace Invop.Saga.Orchestrator.Core.Outbox;

/// <summary>
/// Repository for managing outbox messages with support for transactional guarantees.
/// Implements the Transactional Outbox pattern for reliable message delivery.
/// </summary>
public interface IOutboxRepository
{
    /// <summary>
    /// Saves a new outbox message. Returns true if successful, false if duplicate based on IdempotencyKey.
    /// </summary>
    /// <param name="outboxMessage">The outbox message to save</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if saved successfully, false if duplicate detected</returns>
    ValueTask<bool> SaveOutboxMessageAsync(OutboxMessageWrapper outboxMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all pending outbox messages ready for processing.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of pending outbox messages</returns>
    ValueTask<IEnumerable<OutboxMessageWrapper>> GetPendingOutboxMessagesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as successfully published and sets TTL for auto-cleanup.
    /// </summary>
    /// <param name="idempotencyKey">The unique idempotency key of the message</param>
    /// <param name="ttlSeconds">Time-to-live in seconds for auto-cleanup (default: 3600 = 1 hour)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    ValueTask MarkAsPublishedAsync(string idempotencyKey, int ttlSeconds = 3600, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as failed and increments attempt count.
    /// </summary>
    /// <param name="idempotencyKey">The unique idempotency key of the message</param>
    /// <param name="errorMessage">Error details for diagnostic purposes</param>
    /// <param name="cancellationToken">Cancellation token</param>
    ValueTask MarkAsFailedAsync(string idempotencyKey, string errorMessage, CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a message as processing to prevent concurrent processing.
    /// </summary>
    /// <param name="idempotencyKey">The unique idempotency key of the message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    ValueTask MarkAsProcessingAsync(string idempotencyKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a specific outbox message. Used for manual cleanup or immediate removal after publishing.
    /// </summary>
    /// <param name="idempotencyKey">The unique idempotency key of the message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    ValueTask DeleteAsync(string idempotencyKey, CancellationToken cancellationToken = default);
}