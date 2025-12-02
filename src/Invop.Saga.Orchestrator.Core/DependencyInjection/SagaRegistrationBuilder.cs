using Microsoft.Extensions.DependencyInjection;

namespace Invop.Saga.Orchestrator.Core.DependencyInjection;

/// <summary>
/// Implements ISagaRegistrationBuilder to allow only saga registration in DI.
/// </summary>
internal class SagaRegistrationBuilder : ISagaRegistrationBuilder
{
    private readonly IServiceCollection _services;

    public SagaRegistrationBuilder(IServiceCollection services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    /// <inheritdoc />
    public ISagaRegistrationBuilder AddSaga<TStateMachine, TInstance>()
        where TStateMachine : SagaStateMachine<TInstance>
        where TInstance : class, ISagaInstance
    {
        _services.AddSingleton<TStateMachine>();
        _services.PostConfigure<IServiceProvider>(provider =>
        {
            var registry = provider.GetRequiredService<ISagaRegistry>();
            var stateMachine = provider.GetRequiredService<TStateMachine>();
            registry.RegisterSagaStateMachine(stateMachine);
        });
        return this;
    }
}
