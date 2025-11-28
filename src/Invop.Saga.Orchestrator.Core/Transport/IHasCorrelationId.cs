namespace Invop.Saga.Orchestrator.Core.Transport;

public interface IHasCorrelationId
{
    string CorrelationId { get; set; }
}
