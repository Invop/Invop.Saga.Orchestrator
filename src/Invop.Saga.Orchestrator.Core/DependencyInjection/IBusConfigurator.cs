using Microsoft.Extensions.DependencyInjection;

namespace Invop.Saga.Orchestrator.Core.DependencyInjection;

public interface IBusConfigurator
{
    IServiceCollection Services { get; }
}
