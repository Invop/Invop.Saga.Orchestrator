using Invop.Saga.Orchestrator.Core.Transport;

namespace Invop.Saga.Orchestrator.Core.Attributes;

/// <summary>
/// Defines a compensating action for a saga handler.
/// Compensable transactions can be undone if the saga fails.
/// </summary>
/// <remarks>
/// Based on: https://learn.microsoft.com/azure/architecture/patterns/saga
/// 
/// Compensable transactions occur before the pivot point and can be rolled back.
/// Compensations are executed in reverse order relative to forward transactions.
/// 
/// The compensation handler type must implement <see cref="ISagaMessageHandler{TMessage}"/>
/// with the appropriate compensation message type.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public sealed class CompensationAttribute : Attribute
{
    /// <summary>
    /// The type of the compensation handler.
    /// Must implement <see cref="ISagaMessageHandler{TMessage}"/> with the corresponding compensation message type.
    /// </summary>
    public Type CompensationHandlerType { get; }

    /// <summary>
    /// Indicates whether the compensation is mandatory.
    /// If <see langword="true"/> and the compensation fails, the entire saga is considered failed.
    /// If <see langword="false"/> and the compensation fails, the error is logged but the saga continues.
    /// </summary>
    /// <value>
    /// Default is <see langword="true"/>.
    /// </value>
    public bool IsMandatory { get; init; } = true;

    /// <summary>
    /// Maximum time to wait for compensation execution in milliseconds.
    /// Zero means unlimited wait time.
    /// </summary>
    /// <remarks>
    /// If the compensation exceeds this timeout:
    /// - For mandatory compensations: saga fails
    /// - For optional compensations: error is logged and saga continues
    /// </remarks>
    /// <value>
    /// Default is 0 (no timeout).
    /// </value>
    public int TimeoutMilliseconds { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompensationAttribute"/> class.
    /// </summary>
    /// <param name="compensationHandlerType">
    /// The type of the compensation handler.
    /// Must implement <see cref="ISagaMessageHandler{TMessage}"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="compensationHandlerType"/> is <see langword="null"/>.
    /// </exception>
    public CompensationAttribute(Type compensationHandlerType)
    {
        ArgumentNullException.ThrowIfNull(compensationHandlerType);
        CompensationHandlerType = compensationHandlerType;
    }
}
