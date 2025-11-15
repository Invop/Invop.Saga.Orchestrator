namespace Invop.Saga.Orchestrator.Core.Abstractions;

public interface IHasCorrelationId
{
    string CorrelationId { get; set; }
}
