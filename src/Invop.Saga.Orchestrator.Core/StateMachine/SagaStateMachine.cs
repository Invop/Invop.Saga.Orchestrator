using Invop.Saga.Orchestrator.Core.Transport;

namespace Invop.Saga.Orchestrator.Core.StateMachine;

/// <summary>
/// Base class for building strongly-typed saga state machines using ISagaInstance, SagaState, and ISagaMessage contracts.
/// </summary>
/// <typeparam name="TSagaInstance">Saga instance type</typeparam>
public abstract class SagaStateMachine<TSagaInstance>
    where TSagaInstance : ISagaInstance
{
    private readonly Dictionary<SagaState, List<Func<TSagaInstance, IMessageContext<ISagaMessage>, Task>>> _stateHandlers = new();
    private readonly List<Func<TSagaInstance, IMessageContext<ISagaMessage>, Task>> _initialHandlers = new();

    /// <summary>
    /// Configure handlers for the initial state.
    /// </summary>
    protected void Initially(Action<IConfigurator> config)
    {
        var configurator = new Configurator(_initialHandlers);
        config(configurator);
    }

    /// <summary>
    /// Configure handlers for a specific state.
    /// </summary>
    protected void During(SagaState state, Action<IConfigurator> config)
    {
        if (!_stateHandlers.TryGetValue(state, out var handlers))
        {
            handlers = new List<Func<TSagaInstance, IMessageContext<ISagaMessage>, Task>>();
            _stateHandlers[state] = handlers;
        }

        var configurator = new Configurator(handlers);
        config(configurator);
    }

    /// <summary>
    /// Transitions the saga to a new state.
    /// </summary>
    protected void TransitionTo(TSagaInstance instance, SagaState state)
    {
        ArgumentNullException.ThrowIfNull(instance, nameof(instance));
        instance.CurrentState = state;
    }

    /// <summary>
    /// Publishes a message. Override to implement actual publishing logic.
    /// </summary>
    protected virtual Task PublishAsync(ISagaMessage message)
        => Task.CompletedTask;

    /// <summary>
    /// Handles an incoming message in the current state.
    /// </summary>
    public async Task HandleAsync(TSagaInstance instance, IMessageContext<ISagaMessage> context)
    {
        ArgumentNullException.ThrowIfNull(instance, nameof(instance));
        ArgumentNullException.ThrowIfNull(context, nameof(context));

        var state = instance.CurrentState ?? SagaState.Initial;
        List<Func<TSagaInstance, IMessageContext<ISagaMessage>, Task>>? handlers;
        if (state == SagaState.Initial)
        {
            handlers = _initialHandlers;
        }
        else
        {
            _stateHandlers.TryGetValue(state, out handlers);
        }

        if (handlers != null)
        {
            foreach (var handler in handlers)
            {
                await handler(instance, context);
            }
        }
    }
    /// <summary>
    /// Returns all message types handled by this saga state machine (via When&lt;TMessage&gt; in configuration).
    /// </summary>
    internal IEnumerable<Type> GetHandledMessageTypes()
    {
        var types = new HashSet<Type>();
        // Initial state handlers
        foreach (var del in _initialHandlers)
        {
            if (del.Target is Configurator configurator)
            {
                types.UnionWith(configurator.GetRegisteredMessageTypes());
            }
        }
        // CurrentState handlers
        foreach (var stateList in _stateHandlers.Values)
        {
            foreach (var del in stateList)
            {
                if (del.Target is Configurator configurator)
                {
                    types.UnionWith(configurator.GetRegisteredMessageTypes());
                }
            }
        }

        return types;
    }
    /// <summary>
    /// Fluent configuration interface for state handlers.
    /// </summary>
    public interface IConfigurator
    {
        void When<TMessage>(Func<TSagaInstance, IMessageContext<TMessage>, Task> handler)
            where TMessage : ISagaMessage;
        void Then(Func<TSagaInstance, IMessageContext<ISagaMessage>, Task> action);
        void Publish(Func<TSagaInstance, ISagaMessage> messageFactory);
        void TransitionTo(SagaState state);
        IEnumerable<Type> GetRegisteredMessageTypes();
    }

    private class Configurator : IConfigurator
    {
        private readonly List<Func<TSagaInstance, IMessageContext<ISagaMessage>, Task>> _handlers;
        private readonly HashSet<Type> _registeredMessageTypes = new();

        public Configurator(List<Func<TSagaInstance, IMessageContext<ISagaMessage>, Task>> handlers)
        {
            _handlers = handlers;
        }

        public void When<TMessage>(Func<TSagaInstance, IMessageContext<TMessage>, Task> handler)
            where TMessage : ISagaMessage
        {
            _handlers.Add(async (instance, context) =>
            {
                if (context is IMessageContext<TMessage> typedContext)
                {
                    await handler(instance, typedContext);
                }
            });
            _registeredMessageTypes.Add(typeof(TMessage));
        }

        public void Then(Func<TSagaInstance, IMessageContext<ISagaMessage>, Task> action)
        {
            _handlers.Add(action);
        }

        public void Publish(Func<TSagaInstance, ISagaMessage> messageFactory)
        {
            _handlers.Add(async (instance, _) =>
            {
                var message = messageFactory(instance);
                // Actual publish logic should be implemented in derived class
                await Task.CompletedTask;
            });
        }

        public void TransitionTo(SagaState state)
        {
            _handlers.Add((instance, _) =>
            {
                ArgumentNullException.ThrowIfNull(instance, nameof(instance));
                instance.CurrentState = state;
                return Task.CompletedTask;
            });
        }

        public IEnumerable<Type> GetRegisteredMessageTypes() => _registeredMessageTypes;
    }
}
