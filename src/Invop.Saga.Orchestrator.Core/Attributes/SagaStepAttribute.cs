namespace Invop.Saga.Orchestrator.Core.Attributes;

/// <summary>
/// Marks a command handler for use in saga orchestration.
/// This attribute provides metadata about how the step should be executed.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SagaStepAttribute : Attribute
{
    /// <summary>
    /// Gets the unique name of the saga step.
    /// </summary>
    public string StepName { get; private set; }

    /// <summary>
    /// Gets or sets whether this is a pivot step (point of no return).
    /// After a pivot step succeeds, compensations for previous steps are no longer triggered.
    /// Default is <c>false</c>.
    /// </summary>
    /// <remarks>
    /// The pivot transaction can be:
    /// - The last compensatable transaction, OR
    /// - The first retryable transaction
    /// After pivot, the saga is committed and must complete forward.
    /// See: https://learn.microsoft.com/azure/architecture/patterns/saga#key-concepts
    /// </remarks>
    public bool IsPivot { get; init; }

    /// <summary>
    /// Type of transaction according to Saga pattern.
    /// </summary>
    /// <remarks>
    /// - Compensatable: Can be undone (before pivot)
    /// - Pivot: Point of no return
    /// - Retryable: Must succeed eventually (after pivot), must be idempotent
    /// </remarks>
    public SagaTransactionType TransactionType { get; set; } = SagaTransactionType.Compensatable;

    /// <summary>
    /// Minimum delay before compensation can be triggered (milliseconds).
    /// Useful to wait for eventual consistency or async operations to complete.
    /// </summary>
    /// <remarks>
    /// Some operations may need time to propagate before compensation.
    /// Example: Waiting for event sourcing projections to update.
    /// </remarks>
    public int CompensationDelayMs { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SagaStepAttribute"/> class.
    /// </summary>
    /// <param name="stepName">The unique name of the saga step.</param>
    public SagaStepAttribute(string stepName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stepName);
        StepName = stepName;
    }
}

/// <summary>
/// Types of transactions in saga pattern.
/// </summary>
public enum SagaTransactionType
{
    /// <summary>
    /// Can be undone/compensated by another transaction with opposite effect.
    /// These steps occur BEFORE the pivot point.
    /// </summary>
    /// <remarks>
    /// Example: Reserve inventory, hold payment authorization.
    /// If saga fails, these operations will be compensated.
    /// </remarks>
    Compensatable,

    /// <summary>
    /// Point of no return - after this, compensations are no longer triggered.
    /// The saga is committed after this point succeeds.
    /// Can be the last compensatable OR first retryable transaction.
    /// </summary>
    /// <remarks>
    /// Example: Final payment capture, order confirmation sent to customer.
    /// After this point, saga must complete forward even if errors occur.
    /// </remarks>
    Pivot,

    /// <summary>
    /// Must succeed eventually through retries, cannot be compensated.
    /// These steps occur AFTER the pivot point.
    /// MUST be idempotent to handle retries safely.
    /// </summary>
    /// <remarks>
    /// Example: Send notification, update analytics, create audit log.
    /// These operations will be retried until successful.
    /// </remarks>
    Retryable
}