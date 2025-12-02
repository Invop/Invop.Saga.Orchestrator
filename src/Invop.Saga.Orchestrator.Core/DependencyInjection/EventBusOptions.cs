namespace Invop.Saga.Orchestrator.Core.DependencyInjection;

/// <summary>
/// Configuration options for the event bus.
/// </summary>
public class EventBusOptions
{
    /// <summary>
    /// Gets or sets the subscription client name used for identifying this service instance.
    /// </summary>
    public string SubscriptionClientName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether this instance only publishes messages without subscribing.
    /// </summary>
    public bool IsPublishOnly { get; set; }
}
