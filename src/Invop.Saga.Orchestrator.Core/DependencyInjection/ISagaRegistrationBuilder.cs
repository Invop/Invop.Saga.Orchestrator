using Invop.Saga.Orchestrator.Core.Transport;

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

    /// <summary>
    /// Registers a message handler for a specific saga step message type.
    /// </summary>
    /// <typeparam name="TStep">The saga message type.</typeparam>
    /// <typeparam name="THandler">The handler implementation type.</typeparam>
    /// <returns>The builder for fluent chaining.</returns>
    ISagaRegistrationBuilder AddHandler<TStep, THandler>()
        where TStep : class, ISagaMessage
        where THandler : class, ISagaMessageHandler<TStep>;

}
