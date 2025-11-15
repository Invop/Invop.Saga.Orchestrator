namespace Invop.Saga.Orchestrator.Core.Abstractions;

public interface ISagaStepHandler<TStep> where TStep : class, ISagaStep
{
    ValueTask HandleAsync(ISagaStepContext<TStep> step, CancellationToken cancellationToken = default);
    ValueTask RollbackAsync(ISagaStepContext<TStep> step, CancellationToken cancellationToken = default);
}
