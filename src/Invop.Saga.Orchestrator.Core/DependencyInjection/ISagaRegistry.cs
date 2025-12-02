namespace Invop.Saga.Orchestrator.Core.DependencyInjection;

/// <summary>
/// Saga registry for routing messages to the appropriate saga types.
/// </summary>
public interface ISagaRegistry
{
    /// <summary>
    /// Gets all saga types that can handle the specified message type.
    /// </summary>
    IEnumerable<Type> GetSagaTypesForMessage(Type messageType);

    /// <summary>
    /// Registers a saga state machine and all message types it can handle (using its configuration).
    /// </summary>
    void RegisterSagaStateMachine<TSagaInstance>(SagaStateMachine<TSagaInstance> stateMachine)
        where TSagaInstance : ISagaInstance;
}
