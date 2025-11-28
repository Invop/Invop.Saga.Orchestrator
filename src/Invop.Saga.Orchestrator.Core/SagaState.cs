namespace Invop.Saga.Orchestrator.Core;

/// <summary>
/// Base record for representing saga state
/// </summary>
public record SagaState(string Name)
{
    public static readonly SagaState Initial = new("Initial");
    public static readonly SagaState Final = new("Final");
}