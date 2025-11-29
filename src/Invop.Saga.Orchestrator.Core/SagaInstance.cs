using Invop.Saga.Orchestrator.Core.Transport;

namespace Invop.Saga.Orchestrator.Core;

public abstract class SagaInstance : ISagaInstance
{
    public string TriggerMessageId { get; }

    public string CorrelationId { get; }

    public string InstanceId { get; }

    public required SagaState CurrentState { get; set; } = SagaState.Initial;

    protected SagaInstance(
    string instanceId,
    string triggerMessageId,
    string correlationId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(instanceId, nameof(instanceId));
        ArgumentException.ThrowIfNullOrWhiteSpace(triggerMessageId, nameof(triggerMessageId));
        ArgumentException.ThrowIfNullOrWhiteSpace(correlationId, nameof(correlationId));

        InstanceId = instanceId;
        TriggerMessageId = triggerMessageId;
        CorrelationId = correlationId;
    }

    public bool CanProcess<TM>(IMessageContext<TM> messageContext) where TM : ISagaMessage
    {
        throw new NotImplementedException();
    }
}
