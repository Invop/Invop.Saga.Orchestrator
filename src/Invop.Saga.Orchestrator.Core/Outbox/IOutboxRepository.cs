namespace Invop.Saga.Orchestrator.Core.Outbox;

public interface IOutboxRepository
{
    ValueTask SaveOutboxMessageAsync(OutboxMessageWrapper outboxMessage, CancellationToken cancellationToken = default);
    ValueTask<IEnumerable<OutboxMessageWrapper>> GetPendingOutboxMessagesAsync(CancellationToken cancellationToken = default);
}