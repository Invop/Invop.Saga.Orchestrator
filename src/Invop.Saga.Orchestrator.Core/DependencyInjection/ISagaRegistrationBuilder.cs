namespace Invop.Saga.Orchestrator.Core.DependencyInjection;

/// <summary>
/// Provides a high-level interface for registering saga state machines in the DI container.
/// </summary>
public interface ISagaRegistrationBuilder
{
    /// <summary>
    /// Registers a saga state machine and its instance type.
    /// </summary>
    /// <typeparam name="TStateMachine">Saga state machine type</typeparam>
    /// <typeparam name="TInstance">Saga instance type</typeparam>
    ISagaRegistrationBuilder AddSaga<TStateMachine, TInstance>()
        where TStateMachine : SagaStateMachine<TInstance>
        where TInstance : class, ISagaInstance;
}
