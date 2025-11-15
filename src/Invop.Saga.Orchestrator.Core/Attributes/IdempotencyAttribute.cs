namespace Invop.Saga.Orchestrator.Core.Attributes;

/// <summary>
/// Configures idempotency behavior for saga step handlers.
/// Ensures that retrying the same operation doesn't change the result.
/// </summary>
/// <remarks>
/// Idempotency is critical for retryable transactions (especially after pivot point).
/// The idempotency key is automatically computed from properties marked with <see cref="IdempotencyKeyAttribute"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class IdempotencyAttribute : Attribute
{
    /// <summary>
    /// Enables idempotency checks for this handler.
    /// When enabled, duplicate operations are detected and handled according to <see cref="Behavior"/>.
    /// </summary>
    /// <value>
    /// Default is <see langword="true"/>.
    /// </value>
    public bool Enabled { get; init; } = true;

    /// <summary>
    /// Time-to-live for idempotency records in seconds.
    /// After this period, the idempotency key expires and the operation can be retried.
    /// </summary>
    /// <remarks>
    /// Choose TTL based on:
    /// - Business requirements (how long to prevent duplicates)
    /// - Storage costs (longer TTL = more storage)
    /// - Retry window (should be longer than max retry period)
    /// </remarks>
    /// <value>
    /// Default is 86400 seconds (24 hours).
    /// </value>
    public int TtlSeconds { get; init; } = 86400; // 24 hours

    /// <summary>
    /// Storage strategy for idempotency keys.
    /// Determines where and how idempotency keys are stored.
    /// </summary>
    /// <value>
    /// Default is <see cref="IdempotencyStorageStrategy.InMemory"/>.
    /// </value>
    public IdempotencyStorageStrategy StorageStrategy { get; init; } = IdempotencyStorageStrategy.InMemory;

    /// <summary>
    /// Behavior when an idempotent operation is detected.
    /// Controls what happens when a duplicate operation is identified.
    /// </summary>
    /// <value>
    /// Default is <see cref="IdempotentBehavior.SkipExecution"/>.
    /// </value>
    public IdempotentBehavior Behavior { get; init; } = IdempotentBehavior.SkipExecution;
}

/// <summary>
/// Strategies for storing idempotency keys.
/// </summary>
public enum IdempotencyStorageStrategy
{
    /// <summary>
    /// Store idempotency keys in memory.
    /// Suitable for single-instance deployments or testing.
    /// Keys are lost on application restart.
    /// </summary>
    /// <remarks>
    /// Pros: Fast, no external dependencies
    /// Cons: Not shared across instances, lost on restart
    /// </remarks>
    InMemory,

    /// <summary>
    /// Store idempotency keys in distributed cache (e.g., Redis, Memcached).
    /// Recommended for multi-instance deployments in production.
    /// Keys are shared across all application instances.
    /// </summary>
    /// <remarks>
    /// Pros: Shared across instances, fast, TTL support
    /// Cons: Requires external cache infrastructure
    /// </remarks>
    Distributed,

    /// <summary>
    /// Store idempotency keys in the saga state persistence layer.
    /// Uses the same storage as saga state (database, event store, etc.).
    /// </summary>
    /// <remarks>
    /// Pros: Single storage dependency, transactional consistency
    /// Cons: May be slower than cache, requires cleanup for expired keys
    /// </remarks>
    PersistenceLayer
}

/// <summary>
/// Behavior when detecting an idempotent operation (duplicate request).
/// </summary>
public enum IdempotentBehavior
{
    /// <summary>
    /// Skip execution and return immediately without error.
    /// The operation is treated as successful (already completed).
    /// </summary>
    /// <remarks>
    /// Use when: The operation is truly idempotent and safe to skip.
    /// Example: Sending a notification that was already sent.
    /// </remarks>
    SkipExecution,

    /// <summary>
    /// Throw an exception indicating duplicate operation.
    /// The saga will fail and trigger compensations.
    /// </summary>
    /// <remarks>
    /// Use when: Duplicates indicate a serious problem in orchestration logic.
    /// Example: Attempting to charge payment twice (should never happen).
    /// </remarks>
    ThrowException,

    /// <summary>
    /// Log a warning and skip execution.
    /// Similar to <see cref="SkipExecution"/> but with explicit logging.
    /// </summary>
    /// <remarks>
    /// Use when: You want to track duplicate attempts for monitoring.
    /// Example: Inventory reservation that might be retried due to timeouts.
    /// </remarks>
    LogAndSkip
}