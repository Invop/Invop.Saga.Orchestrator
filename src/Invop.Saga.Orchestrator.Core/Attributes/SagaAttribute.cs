namespace Invop.Saga.Orchestrator.Core.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SagaAttribute : Attribute
{
    /// <summary>
    /// Unique name of the saga. Used for identification and routing.
    /// </summary>
    public string Name { get; internal set; }

    /// <summary>
    /// Whether to enable automatic state persistence after each step.
    /// </summary>
    public bool AutoPersist { get; set; } = true;

    /// <summary>
    /// Maximum execution timeout for the entire saga in seconds.
    /// If saga doesn't complete within this time, it will be aborted and compensated.
    /// 0 = no timeout (not recommended for production).
    /// </summary>
    /// <remarks>
    /// Helps prevent long-running sagas from consuming resources indefinitely.
    /// Should be set based on expected saga duration + buffer for retries.
    /// See: https://learn.microsoft.com/azure/architecture/patterns/saga
    /// </remarks>
    public int TimeoutSeconds { get; set; }

    /// <summary>
    /// Strategy for handling data anomalies (dirty reads, lost updates, fuzzy reads).
    /// </summary>
    /// <remarks>
    /// Data anomalies can occur when multiple sagas operate on shared data.
    /// Choose strategy based on your consistency requirements and performance trade-offs.
    /// See: https://learn.microsoft.com/azure/architecture/patterns/saga
    /// </remarks>
    public DataIsolationStrategy IsolationStrategy { get; set; } = DataIsolationStrategy.None;

    /// <summary>
    /// Whether to use semantic locks to prevent concurrent modifications.
    /// When enabled, saga steps will acquire application-level locks.
    /// </summary>
    /// <remarks>
    /// Semantic locking uses a semaphore to indicate that an update is in progress.
    /// Prevents dirty reads by blocking concurrent access to saga participant data.
    /// Trade-off: Reduces concurrency but improves consistency.
    /// See: https://learn.microsoft.com/azure/architecture/patterns/saga
    /// </remarks>
    public bool UseSemanticLocking { get; set; }

    public SagaAttribute(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }
}

/// <summary>
/// Strategies for handling data anomalies in distributed saga transactions.
/// </summary>
public enum DataIsolationStrategy
{
    /// <summary>
    /// No special isolation strategy (default).
    /// Lowest overhead but potential for data anomalies.
    /// </summary>
    None,
}