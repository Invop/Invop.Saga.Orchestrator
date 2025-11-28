using Microsoft.Extensions.DependencyInjection;

namespace Invop.Saga.Orchestrator.Core.DependencyInjection;

public interface IBusConfigurator
{
    IBusConfigurator SetPublishOnly(bool publishOnly = true);
    IServiceCollection Services { get; }
}
