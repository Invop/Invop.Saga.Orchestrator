namespace Invop.Saga.Orchestrator.Core.Outbox;

public interface IOutboxProcessor
{
    ValueTask ProcessPendingMessagesAsync(CancellationToken cancellationToken = default);
}