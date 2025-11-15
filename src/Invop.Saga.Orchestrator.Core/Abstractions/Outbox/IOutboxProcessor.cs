namespace Invop.Saga.Orchestrator.Core.Abstractions.Outbox;

public interface IOutboxProcessor
{
    ValueTask ProcessPendingMessagesAsync(CancellationToken cancellationToken = default);
}