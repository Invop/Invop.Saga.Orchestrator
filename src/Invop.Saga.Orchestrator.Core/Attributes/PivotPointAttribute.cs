namespace Invop.Saga.Orchestrator.Core.Attributes;

/// <summary>
/// Marks a handler as the pivot point (point of no return) in a saga.
/// After the pivot point, all operations must be retryable and cannot have compensations.
/// </summary>
/// <remarks>
/// Based on: https://learn.microsoft.com/azure/architecture/patterns/saga
/// 
/// The pivot point is a critical irreversible operation (e.g., charging payment, shipping goods).
/// 
/// Characteristics:
/// - Idempotent (can be safely retried)
/// - Irreversible (cannot be undone)
/// - Critical to the business process
/// 
/// Operations after the pivot point:
/// - Must be retryable (with <see cref="RetryPolicyAttribute"/>)
/// - Must be idempotent (with <see cref="IdempotencyAttribute"/>)
/// - CANNOT have compensations (validated at startup)
/// 
/// Only one pivot point is allowed per saga.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class PivotPointAttribute : Attribute
{
    /// <summary>
    /// Description of why this operation is irreversible.
    /// Used for documenting business logic and architecture decisions.
    /// </summary>
    /// <remarks>
    /// Examples:
    /// - "Payment charge is irreversible once processed by payment gateway"
    /// - "Order shipment cannot be recalled once picked up by courier"
    /// - "License activation is permanent and tracked by external system"
    /// </remarks>
    public string? Reason { get; private init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PivotPointAttribute"/> class.
    /// </summary>
    public PivotPointAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PivotPointAttribute"/> class with a reason.
    /// </summary>
    /// <param name="reason">Description of why this operation is irreversible.</param>
    public PivotPointAttribute(string? reason)
    {
        Reason = reason;
    }
}
