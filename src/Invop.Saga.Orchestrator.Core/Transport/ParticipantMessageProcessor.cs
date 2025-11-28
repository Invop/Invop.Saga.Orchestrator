using Invop.Saga.Orchestrator.Core.Outbox;

namespace Invop.Saga.Orchestrator.Core.Transport;

internal class ParticipantMessageProcessor : IMessageProcessor
{
    public ValueTask ProcessMessageAsync(OutboxMessageWrapper outboxMessage, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}
