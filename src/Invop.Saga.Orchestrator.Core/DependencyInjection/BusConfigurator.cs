using Microsoft.Extensions.DependencyInjection;

namespace Invop.Saga.Orchestrator.Core.DependencyInjection;

internal class BusConfigurator : IBusConfigurator
{
    public BusConfigurator(IServiceCollection services)
    {
        Services = services ?? throw new ArgumentNullException(nameof(services));
    }

    public IServiceCollection Services { get; }

    public IBusConfigurator SetPublishOnly(bool publishOnly = true)
    {
        throw new NotImplementedException();
    }
}
