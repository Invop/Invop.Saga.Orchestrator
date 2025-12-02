using System.Collections.Concurrent;

namespace Invop.Saga.Orchestrator.Core.DependencyInjection;

/// <summary>
/// Production-ready saga registry implementation with thread-safe storage and automatic message type analysis.
/// </summary>
public class SagaRegistry : ISagaRegistry
{
    private readonly ConcurrentDictionary<Type, HashSet<Type>> _messageToSagaTypes = new();
    private readonly HashSet<Type> _registeredSagas = new();

    /// <inheritdoc />
    public IEnumerable<Type> GetSagaTypesForMessage(Type messageType)
        => _messageToSagaTypes.TryGetValue(messageType, out var sagaTypes) ? sagaTypes : Array.Empty<Type>();

    /// <inheritdoc />
    public void RegisterSagaStateMachine<TSagaInstance>(SagaStateMachine<TSagaInstance> stateMachine)
        where TSagaInstance : ISagaInstance
    {
        var sagaType = stateMachine.GetType();
        if (!_registeredSagas.Add(sagaType))
        {
            return; // already registered
        }

        var messageTypes = stateMachine.GetHandledMessageTypes();
        foreach (var msgType in messageTypes)
        {
            _messageToSagaTypes.AddOrUpdate(
                msgType,
                _ => [sagaType],
                (_, set) =>
                {
                    set.Add(sagaType);
                    return set;
                });
        }
    }
}
