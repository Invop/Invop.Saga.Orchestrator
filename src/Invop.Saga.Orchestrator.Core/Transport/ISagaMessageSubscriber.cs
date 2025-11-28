namespace Invop.Saga.Orchestrator.Core.Transport;

public interface ISagaMessageSubscriber
{
    ValueTask StartAsync(CancellationToken cancellationToken = default);
    ValueTask StopAsync(CancellationToken cancellationToken = default);
}
public interface ISagaMessageSubscriber<TMessage> where TMessage : class, ISagaMessage
{
    ValueTask StartAsync(CancellationToken cancellationToken = default);
    ValueTask StopAsync(CancellationToken cancellationToken = default);
}
