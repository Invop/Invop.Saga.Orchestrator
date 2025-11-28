namespace Invop.Saga.Orchestrator.Core.Transport;

public interface ISagaMessageHandler<TStep> where TStep : class, ISagaMessage
{
    ValueTask HandleAsync(IMessageContext<TStep> step, CancellationToken cancellationToken = default);
    ValueTask RollbackAsync(IMessageContext<TStep> step, CancellationToken cancellationToken = default);
}
