using Invop.Saga.Orchestrator.Core.Outbox;

namespace Invop.Saga.Orchestrator.Core.Tests.Outbox;

public sealed class OutboxMessageWrapperTests
{
    [Fact]
    public void OutboxMessageWrapper_ShouldInitializeWithDefaultValues_WhenCreated()
    {
        // Arrange & Act
        var message = new OutboxMessageWrapper
        {
            StepName = "TestStep",
            CorrelationId = "corr-123",
            MessageType = "TestMessageType",
            Payload = "test"u8.ToArray(),
            IdempotencyKey = "key-123",
            SenderId = "sender-1"
        };

        // Assert
        message.StepName.ShouldBe("TestStep");
        message.CorrelationId.ShouldBe("corr-123");
        message.MessageType.ShouldBe("TestMessageType");
        message.Payload.ShouldBe("test"u8.ToArray());
        message.IdempotencyKey.ShouldBe("key-123");
        message.SenderId.ShouldBe("sender-1");
        message.State.ShouldBe(OutboxMessageState.Pending);
        message.AttemptCount.ShouldBe(0);
        message.Ttl.ShouldBe(-1);
        message.ProcessedOnUtc.ShouldBeNull();
        message.ErrorMessage.ShouldBeNull();
        message.CreatedAt.ShouldBeInRange(DateTime.UtcNow.AddSeconds(-5), DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void OutboxMessageWrapper_ShouldAllowStateChange_WhenStateIsSet()
    {
        // Arrange
        var message = CreateTestMessage();

        // Act
        message.State = OutboxMessageState.Processing;

        // Assert
        message.State.ShouldBe(OutboxMessageState.Processing);
    }

    [Fact]
    public void OutboxMessageWrapper_ShouldAllowAttemptCountIncrement_WhenAttemptCountIsSet()
    {
        // Arrange
        var message = CreateTestMessage();

        // Act
        message.AttemptCount = 3;

        // Assert
        message.AttemptCount.ShouldBe(3);
    }

    [Fact]
    public void OutboxMessageWrapper_ShouldAllowErrorMessageSet_WhenErrorMessageIsSet()
    {
        // Arrange
        var message = CreateTestMessage();

        // Act
        message.ErrorMessage = "Test error";

        // Assert
        message.ErrorMessage.ShouldBe("Test error");
    }

    [Fact]
    public void OutboxMessageWrapper_ShouldAllowTtlChange_WhenTtlIsSet()
    {
        // Arrange
        var message = CreateTestMessage();

        // Act
        message.Ttl = 3600;

        // Assert
        message.Ttl.ShouldBe(3600);
    }

    [Theory]
    [InlineData(OutboxMessageState.Pending)]
    [InlineData(OutboxMessageState.Processing)]
    [InlineData(OutboxMessageState.Published)]
    [InlineData(OutboxMessageState.Failed)]
    public void OutboxMessageState_ShouldSupportAllStates_WhenStateIsSet(OutboxMessageState state)
    {
        // Arrange
        var message = CreateTestMessage();

        // Act
        message.State = state;

        // Assert
        message.State.ShouldBe(state);
    }

    [Fact]
    public void OutboxMessageWrapper_ShouldHaveImmutableRequiredProperties_WhenInitialized()
    {
        // Arrange
        var stepName = "ImmutableStep";
        var correlationId = "immutable-corr";
        var messageType = "ImmutableType";
        var payload = "immutable"u8.ToArray();
        var idempotencyKey = "immutable-key";
        var senderId = "immutable-sender";

        // Act
        var message = new OutboxMessageWrapper
        {
            StepName = stepName,
            CorrelationId = correlationId,
            MessageType = messageType,
            Payload = payload,
            IdempotencyKey = idempotencyKey,
            SenderId = senderId
        };

        // Assert
        message.StepName.ShouldBe(stepName);
        message.CorrelationId.ShouldBe(correlationId);
        message.MessageType.ShouldBe(messageType);
        message.Payload.ShouldBe(payload);
        message.IdempotencyKey.ShouldBe(idempotencyKey);
        message.SenderId.ShouldBe(senderId);
    }

    private static OutboxMessageWrapper CreateTestMessage()
    {
        return new OutboxMessageWrapper
        {
            StepName = "TestStep",
            CorrelationId = "test-correlation",
            MessageType = "TestType",
            Payload = "test"u8.ToArray(),
            IdempotencyKey = "test-key",
            SenderId = "test-sender"
        };
    }
}
