using Invop.Saga.Orchestrator.Core.Outbox;

namespace Invop.Saga.Orchestrator.Core.Transport;

internal interface IMessageProcessor
{
    ValueTask ProcessMessageAsync(OutboxMessageWrapper outboxMessage,
        CancellationToken cancellationToken = default);
}
