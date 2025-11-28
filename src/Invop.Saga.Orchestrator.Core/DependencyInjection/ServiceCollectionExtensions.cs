using Microsoft.Extensions.DependencyInjection;

namespace Invop.Saga.Orchestrator.Core.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSagaOrchestratorCore(this IServiceCollection services,
        Action<IBusConfigurator>? busConfigurator = null)
    {
        // Register core services here

        var builder = new BusConfigurator(services);
        busConfigurator?.Invoke(builder);
        return services;
    }
}
