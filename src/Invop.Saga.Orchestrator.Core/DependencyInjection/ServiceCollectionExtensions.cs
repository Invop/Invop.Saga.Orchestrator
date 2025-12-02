using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
namespace Invop.Saga.Orchestrator.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{

    /// <summary>
    /// Registers core services, bus configuration, and saga state machines in the DI container.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="busConfigurator">Optional bus configuration action</param>
    /// <param name="sagaConfigurator">Optional saga registration action</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddSagaOrchestratorCore(this IServiceCollection services,
        Action<IBusConfigurator>? busConfigurator = null,
        Action<ISagaRegistrationBuilder>? sagaConfigurator = null)
    {

        services.AddSingleton<ISagaRegistry, SagaRegistry>();

        var builder = new BusConfigurator(services);
        busConfigurator?.Invoke(builder);

        var sagaBuilder = new SagaRegistrationBuilder(services);
        sagaConfigurator?.Invoke(sagaBuilder);

        return services;
    }

    /// <summary>
    /// Registers core services, bus configuration, and saga state machines in the DI container with configuration binding.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration section containing EventBusOptions</param>
    /// <param name="busConfigurator">Optional bus configuration action</param>
    /// <param name="sagaConfigurator">Optional saga registration action</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddSagaOrchestratorCore(this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusConfigurator>? busConfigurator = null,
        Action<ISagaRegistrationBuilder>? sagaConfigurator = null)
    {
        services.Configure<EventBusOptions>(configuration);
        return services.AddSagaOrchestratorCore(busConfigurator, sagaConfigurator);
    }

    /// <summary>
    /// Registers core services, bus configuration, and saga state machines in the DI container with options configuration.
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Action to configure EventBusOptions</param>
    /// <param name="busConfigurator">Optional bus configuration action</param>
    /// <param name="sagaConfigurator">Optional saga registration action</param>
    /// <returns>Service collection</returns>
    public static IServiceCollection AddSagaOrchestratorCore(this IServiceCollection services,
        Action<EventBusOptions> configureOptions,
        Action<IBusConfigurator>? busConfigurator = null,
        Action<ISagaRegistrationBuilder>? sagaConfigurator = null)
    {
        services.Configure(configureOptions);
        return services.AddSagaOrchestratorCore(busConfigurator, sagaConfigurator);
    }
}