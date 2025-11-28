namespace Invop.Saga.Orchestrator.Core.Transport;

public interface IMessageContext<TSagaMessage>
    where TSagaMessage : ISagaMessage
{
    TSagaMessage Message { get; }
    string MessageId { get; }
    string CorrelationId { get; }
    string SenderId { get; }
}
