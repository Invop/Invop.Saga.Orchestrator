namespace Invop.Saga.Orchestrator.Core.Transport;

public interface ISagaMessageHandler<TStep> where TStep : class, ISagaMessage
{
    /// <summary>
    /// Handles the saga message (forward execution).
    /// </summary>
    /// <param name="context">Message context containing the message and metadata.</param>
    /// <param name="cancellationToken">Cancellation token for operation cancellation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask HandleAsync(IMessageContext<TStep> context, CancellationToken cancellationToken = default);
}
