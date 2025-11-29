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

        if (sagaConfigurator is not null)
        {
            var sagaBuilder = new SagaRegistrationBuilder(services);
            sagaConfigurator(sagaBuilder);
        }

        return services;
    }
}
