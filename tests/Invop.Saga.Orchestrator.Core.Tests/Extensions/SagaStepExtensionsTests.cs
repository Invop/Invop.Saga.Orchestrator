using Invop.Saga.Orchestrator.Core.Abstractions;
using Invop.Saga.Orchestrator.Core.Attributes;
using Invop.Saga.Orchestrator.Core.Extensions;

namespace Invop.Saga.Orchestrator.Core.Tests.Extensions;

public sealed class SagaStepExtensionsTests
{
    [Fact]
    public void GetIdempotencyKey_ShouldThrowArgumentNullException_WhenSagaStepIsNull()
    {
        // Arrange
        ISagaStep sagaStep = null!;

        // Act & Assert
        Should.Throw<ArgumentNullException>(() => sagaStep.GetIdempotencyKey());
    }

    [Fact]
    public void GetIdempotencyKey_ShouldReturnHashOfCorrelationId_WhenNoPropertiesAreDecorated()
    {
        // Arrange
        var sagaStep = new TestSagaStepNoAttributes
        {
            CorrelationId = "test-correlation-id",
            RequestId = "test-request-id"
        };

        // Act
        var key = sagaStep.GetIdempotencyKey();

        // Assert
        key.ShouldNotBeNullOrWhiteSpace();
        key.ShouldBe("test-correlation-id".ComputeSha256Hash());
    }

    [Fact]
    public void GetIdempotencyKey_ShouldIncludeSingleDecoratedProperty_WhenOnePropertyHasAttribute()
    {
        // Arrange
        var sagaStep = new TestSagaStepSingleProperty
        {
            CorrelationId = "correlation-123",
            RequestId = "request-456",
            OrderId = "order-789"
        };

        // Act
        var key = sagaStep.GetIdempotencyKey();

        // Assert
        key.ShouldNotBeNullOrWhiteSpace();
        var expectedInput = "correlation-123order-789";
        key.ShouldBe(expectedInput.ComputeSha256Hash());
    }

    [Fact]
    public void GetIdempotencyKey_ShouldIncludeMultipleProperties_WhenMultiplePropertiesAreDecorated()
    {
        // Arrange
        var sagaStep = new TestSagaStepMultipleProperties
        {
            CorrelationId = "corr-001",
            RequestId = "req-001",
            CustomerId = "cust-100",
            ProductId = "prod-200",
            Amount = 99.99m
        };

        // Act
        var key = sagaStep.GetIdempotencyKey();

        // Assert
        key.ShouldNotBeNullOrWhiteSpace();
        var expectedInput = $"corr-001cust-100prod-200{sagaStep.Amount}";
        key.ShouldBe(expectedInput.ComputeSha256Hash());
    }

    [Fact]
    public void GetIdempotencyKey_ShouldOrderPropertiesCorrectly_WhenOrderIsSpecified()
    {
        // Arrange
        var sagaStep = new TestSagaStepWithOrder
        {
            CorrelationId = "corr",
            RequestId = "req",
            ThirdProperty = "third",
            FirstProperty = "first",
            SecondProperty = "second"
        };

        // Act
        var key = sagaStep.GetIdempotencyKey();

        // Assert
        key.ShouldNotBeNullOrWhiteSpace();
        // Properties should be ordered: FirstProperty (1), SecondProperty (2), ThirdProperty (3)
        var expectedInput = "corrfirstsecondthird";
        key.ShouldBe(expectedInput.ComputeSha256Hash());
    }

    [Fact]
    public void GetIdempotencyKey_ShouldExcludeCorrelationIdProperty_WhenCorrelationIdIsDecorated()
    {
        // Arrange
        var sagaStep = new TestSagaStepWithDecoratedCorrelationId
        {
            CorrelationId = "correlation-xyz",
            RequestId = "request-xyz",
            UserId = "user-123"
        };

        // Act
        var key = sagaStep.GetIdempotencyKey();

        // Assert
        key.ShouldNotBeNullOrWhiteSpace();
        // CorrelationId should be included at the start but not as a decorated property
        var expectedInput = "correlation-xyzuser-123";
        key.ShouldBe(expectedInput.ComputeSha256Hash());
    }

    [Fact]
    public void GetIdempotencyKey_ShouldIgnoreNonDecoratedProperties_WhenMixedPropertiesExist()
    {
        // Arrange
        var sagaStep = new TestSagaStepMixedProperties
        {
            CorrelationId = "corr-mixed",
            RequestId = "req-mixed",
            DecoratedProperty = "decorated",
            NonDecoratedProperty = "not-included"
        };

        // Act
        var key = sagaStep.GetIdempotencyKey();

        // Assert
        key.ShouldNotBeNullOrWhiteSpace();
        var expectedInput = "corr-mixeddecorated";
        key.ShouldBe(expectedInput.ComputeSha256Hash());
    }

    [Fact]
    public void GetIdempotencyKey_ShouldHandleNullPropertyValues_WhenPropertyValueIsNull()
    {
        // Arrange
        var sagaStep = new TestSagaStepWithNullableProperty
        {
            CorrelationId = "corr-null-test",
            RequestId = "req-null-test",
            NullableProperty = null
        };

        // Act
        var key = sagaStep.GetIdempotencyKey();

        // Assert
        key.ShouldNotBeNullOrWhiteSpace();
        var expectedInput = "corr-null-test";
        key.ShouldBe(expectedInput.ComputeSha256Hash());
    }

    [Fact]
    public void GetIdempotencyKey_ShouldBeDeterministic_WhenCalledMultipleTimes()
    {
        // Arrange
        var sagaStep = new TestSagaStepMultipleProperties
        {
            CorrelationId = "deterministic-test",
            RequestId = "req-det",
            CustomerId = "cust-det",
            ProductId = "prod-det",
            Amount = 123.45m
        };

        // Act
        var key1 = sagaStep.GetIdempotencyKey();
        var key2 = sagaStep.GetIdempotencyKey();
        var key3 = sagaStep.GetIdempotencyKey();

        // Assert
        key1.ShouldBe(key2);
        key2.ShouldBe(key3);
    }

    [Fact]
    public void GetIdempotencyKey_ShouldProduceDifferentKeys_WhenPropertyValuesAreDifferent()
    {
        // Arrange
        var sagaStep1 = new TestSagaStepSingleProperty
        {
            CorrelationId = "corr-001",
            RequestId = "req-001",
            OrderId = "order-001"
        };

        var sagaStep2 = new TestSagaStepSingleProperty
        {
            CorrelationId = "corr-001",
            RequestId = "req-001",
            OrderId = "order-002"
        };

        // Act
        var key1 = sagaStep1.GetIdempotencyKey();
        var key2 = sagaStep2.GetIdempotencyKey();

        // Assert
        key1.ShouldNotBe(key2);
    }

    // Test Saga Step Classes

    private sealed class TestSagaStepNoAttributes : ISagaStep
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
    }

    private sealed class TestSagaStepSingleProperty : ISagaStep
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;

        [IdempotencyKey(1)]
        public string OrderId { get; set; } = string.Empty;
    }

    private sealed class TestSagaStepMultipleProperties : ISagaStep
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;

        [IdempotencyKey(1)]
        public string CustomerId { get; set; } = string.Empty;

        [IdempotencyKey(2)]
        public string ProductId { get; set; } = string.Empty;

        [IdempotencyKey(3)]
        public decimal Amount { get; set; }
    }

    private sealed class TestSagaStepWithOrder : ISagaStep
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;

        [IdempotencyKey(1)]
        public string FirstProperty { get; set; } = string.Empty;

        [IdempotencyKey(2)]
        public string SecondProperty { get; set; } = string.Empty;

        [IdempotencyKey(3)]
        public string ThirdProperty { get; set; } = string.Empty;
    }

    private sealed class TestSagaStepWithDecoratedCorrelationId : ISagaStep
    {
        [IdempotencyKey(1)]
        public string CorrelationId { get; set; } = string.Empty;

        public string RequestId { get; set; } = string.Empty;

        [IdempotencyKey(2)]
        public string UserId { get; set; } = string.Empty;
    }

    private sealed class TestSagaStepMixedProperties : ISagaStep
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;

        [IdempotencyKey(1)]
        public string DecoratedProperty { get; set; } = string.Empty;

        public string NonDecoratedProperty { get; set; } = string.Empty;
    }

    private sealed class TestSagaStepWithNullableProperty : ISagaStep
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;

        [IdempotencyKey(1)]
        public string? NullableProperty { get; set; }
    }

    private sealed class TestSagaStepVariousTypes : ISagaStep
    {
        public string CorrelationId { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;

        [IdempotencyKey(1)]
        public int IntProperty { get; set; }

        [IdempotencyKey(2)]
        public decimal DecimalProperty { get; set; }

        [IdempotencyKey(3)]
        public bool BoolProperty { get; set; }

        [IdempotencyKey(4)]
        public DateTime DateTimeProperty { get; set; }
    }
}