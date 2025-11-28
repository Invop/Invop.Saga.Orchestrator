using Invop.Saga.Orchestrator.Core.Transport;

namespace Invop.Saga.Orchestrator.Core;

public interface ISagaInstance
{
    /// <summary>
    /// id of the first message that started the saga execution.
    /// </summary>
    string TriggerMessageId { get; }

    /// <summary>
    /// correlation id across the messages.
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// the current saga instance id.
    /// </summary>
    string InstanceId { get; }

    /// <summary>
    /// Current state of the saga instance.
    /// </summary>
    SagaState State { get; set; }

    /// <summary>
    /// Checks if the saga instance can process the given message context.
    /// </summary>
    bool CanProcess<TM>(IMessageContext<TM> messageContext) where TM : ISagaMessage;
}
