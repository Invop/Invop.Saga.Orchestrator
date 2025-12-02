namespace Invop.Saga.Orchestrator.Core.Attributes;

/// <summary>
/// Defines a timeout for the entire saga.
/// If the saga does not complete within the specified time, compensation is initiated.
/// </summary>
/// <remarks>
/// Based on distributed transaction best practices to prevent stuck sagas.
/// 
/// Saga timeout is different from individual step timeout (<see cref="RetryPolicyAttribute.TimeoutMilliseconds"/>).
/// - Step timeout: Maximum time for a single handler execution
/// - Saga timeout: Maximum time for the entire saga from start to completion
/// 
/// When a saga times out:
/// - All compensable steps are compensated in reverse order
/// - The saga transitions to a failed or suspended state
/// - Monitoring/alerting can be triggered for manual intervention
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class SagaTimeoutAttribute : Attribute
{
    /// <summary>
    /// Maximum duration for saga execution in seconds.
    /// After this time, the saga is considered failed and compensation is triggered.
    /// </summary>
    /// <remarks>
    /// Choose timeout based on:
    /// - Expected normal execution time + buffer
    /// - Business requirements (how long can a transaction be pending)
    /// - Downstream service SLAs
    /// 
    /// Example timeouts:
    /// - Simple order processing: 60-300 seconds
    /// - Complex multi-service workflows: 300-600 seconds
    /// - Long-running business processes: 3600+ seconds
    /// </remarks>
    public int TimeoutSeconds { get; }

    /// <summary>
    /// Action to take when the saga times out.
    /// </summary>
    /// <value>
    /// Default is <see cref="TimeoutBehavior.CompensateAndFail"/>.
    /// </value>
    public TimeoutBehavior Behavior { get; init; } = TimeoutBehavior.CompensateAndFail;

    /// <summary>
    /// Initializes a new instance of the <see cref="SagaTimeoutAttribute"/> class.
    /// </summary>
    /// <param name="timeoutSeconds">
    /// Maximum duration for saga execution in seconds.
    /// Must be positive.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="timeoutSeconds"/> is less than or equal to zero.
    /// </exception>
    public SagaTimeoutAttribute(int timeoutSeconds)
    {
        if (timeoutSeconds <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(timeoutSeconds), "Timeout must be positive.");
        }

        TimeoutSeconds = timeoutSeconds;
    }
}

/// <summary>
/// Behavior when a saga times out.
/// </summary>
public enum TimeoutBehavior
{
    /// <summary>
    /// Trigger compensations and transition the saga to Failed state.
    /// All compensable steps are undone in reverse order.
    /// </summary>
    /// <remarks>
    /// Use when:
    /// - Automatic rollback is safe and desired
    /// - Business rules require cleanup on timeout
    /// - No manual intervention is needed
    /// </remarks>
    CompensateAndFail,

    /// <summary>
    /// Transition the saga to Suspended state for manual intervention.
    /// Compensations are NOT executed automatically.
    /// </summary>
    /// <remarks>
    /// Use when:
    /// - Manual review is required before compensation
    /// - Partial completion might be acceptable
    /// - Business rules require human decision
    /// - Automatic rollback might cause data loss
    /// </remarks>
    Suspend
}
