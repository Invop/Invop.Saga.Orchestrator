using Microsoft.Extensions.DependencyInjection;

namespace Invop.Saga.Orchestrator.Core.Outbox;

/// <summary>
/// Extension methods for registering outbox pattern services.
/// </summary>
public static class OutboxServiceCollectionExtensions
{
    /// <summary>
    /// Adds the outbox pattern services to the service collection.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configureOptions">Optional configuration action for outbox processor options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddOutboxPattern(
        this IServiceCollection services,
        Action<OutboxProcessorOptions>? configureOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Register options
        var options = new OutboxProcessorOptions();
        configureOptions?.Invoke(options);
        services.AddSingleton(options);

        // Register processor
        services.AddScoped<IOutboxProcessor, OutboxProcessor>();

        services.AddHostedService<OutboxBackgroundService>();

        return services;
    }
}
