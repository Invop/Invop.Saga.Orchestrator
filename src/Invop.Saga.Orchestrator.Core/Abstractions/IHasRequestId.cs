namespace Invop.Saga.Orchestrator.Core.Abstractions;

public interface IHasRequestId
{
    string RequestId { get; set; }
}
