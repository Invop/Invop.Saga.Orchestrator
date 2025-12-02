namespace Invop.Saga.Orchestrator.Core.Attributes;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public sealed class IdempotencyKeyAttribute : Attribute
{
    /// <summary>
    /// Order in which this property should be included in the key.
    /// Lower values come first.
    /// </summary>
    public int? Order { get; private init; }

    public IdempotencyKeyAttribute(int? order = null)
    {
        if (order is not null and < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(order), "Order must be non-negative.");
        }

        Order = order;
    }
}