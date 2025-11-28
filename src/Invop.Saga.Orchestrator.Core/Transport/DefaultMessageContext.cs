namespace Invop.Saga.Orchestrator.Core.Transport;

internal class DefaultMessageContext<TSagaMessage> : IMessageContext<TSagaMessage>
    where TSagaMessage : ISagaMessage
{
    public DefaultMessageContext(TSagaMessage message, string messageId, string correlationId, string senderId)
    {
        Message = message ?? throw new ArgumentNullException(nameof(message));
        MessageId = messageId ?? throw new ArgumentNullException(nameof(messageId));
        CorrelationId = correlationId ?? throw new ArgumentNullException(nameof(correlationId));
        SenderId = senderId ?? throw new ArgumentNullException(nameof(senderId));
    }

    public TSagaMessage Message { get; }

    public string MessageId { get; }

    public string CorrelationId { get; }

    public string SenderId { get; }
}
