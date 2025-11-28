namespace Invop.Saga.Orchestrator.Core.Transport;

public interface IHasRequestId
{
    string RequestId { get; set; }
}
