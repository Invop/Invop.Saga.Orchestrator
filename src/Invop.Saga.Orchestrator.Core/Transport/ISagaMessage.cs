namespace Invop.Saga.Orchestrator.Core.Transport;

public interface ISagaMessage : IHasCorrelationId, IHasRequestId;