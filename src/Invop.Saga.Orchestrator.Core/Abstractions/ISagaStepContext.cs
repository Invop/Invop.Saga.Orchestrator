namespace Invop.Saga.Orchestrator.Core.Abstractions;

public interface ISagaStepContext<TSagaStep>
    where TSagaStep : class, ISagaStep
{
    TSagaStep SagaStep { get; }
    string CorrelationId { get; }
    string SenderId { get; }
    string IdempotencyKey { get; }
}
