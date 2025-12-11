using Invop.Saga.Orchestrator.Core.Outbox;

namespace Invop.Saga.Orchestrator.Core.Tests.Outbox;

public sealed class OutboxProcessorOptionsTests
{
    [Fact]
    public void OutboxProcessorOptions_ShouldHaveDefaultValues_WhenCreated()
    {
        // Act
        var options = new OutboxProcessorOptions();

        // Assert
        options.ProcessingInterval.ShouldBe(TimeSpan.FromSeconds(5));
        options.MaxRetryAttempts.ShouldBe(3);
        options.PublishedMessageTtlSeconds.ShouldBe(3600);
        options.BatchSize.ShouldBe(100);
        options.Enabled.ShouldBeTrue();
    }

    [Fact]
    public void OutboxProcessorOptions_ShouldAllowCustomProcessingInterval_WhenSet()
    {
        // Arrange
        var options = new OutboxProcessorOptions();
        var customInterval = TimeSpan.FromSeconds(10);

        // Act
        options.ProcessingInterval = customInterval;

        // Assert
        options.ProcessingInterval.ShouldBe(customInterval);
    }

    [Fact]
    public void OutboxProcessorOptions_ShouldAllowCustomMaxRetryAttempts_WhenSet()
    {
        // Arrange
        var options = new OutboxProcessorOptions();

        // Act
        options.MaxRetryAttempts = 5;

        // Assert
        options.MaxRetryAttempts.ShouldBe(5);
    }

    [Fact]
    public void OutboxProcessorOptions_ShouldAllowCustomTtl_WhenSet()
    {
        // Arrange
        var options = new OutboxProcessorOptions();

        // Act
        options.PublishedMessageTtlSeconds = 7200;

        // Assert
        options.PublishedMessageTtlSeconds.ShouldBe(7200);
    }

    [Fact]
    public void OutboxProcessorOptions_ShouldAllowCustomBatchSize_WhenSet()
    {
        // Arrange
        var options = new OutboxProcessorOptions();

        // Act
        options.BatchSize = 50;

        // Assert
        options.BatchSize.ShouldBe(50);
    }

    [Fact]
    public void OutboxProcessorOptions_ShouldAllowDisabling_WhenEnabledIsSetToFalse()
    {
        // Arrange
        var options = new OutboxProcessorOptions();

        // Act
        options.Enabled = false;

        // Assert
        options.Enabled.ShouldBeFalse();
    }

    [Fact]
    public void OutboxProcessorOptions_ShouldAllowAllPropertiesConfiguration_WhenCreated()
    {
        // Act
        var options = new OutboxProcessorOptions
        {
            ProcessingInterval = TimeSpan.FromSeconds(15),
            MaxRetryAttempts = 10,
            PublishedMessageTtlSeconds = 1800,
            BatchSize = 200,
            Enabled = false
        };

        // Assert
        options.ProcessingInterval.ShouldBe(TimeSpan.FromSeconds(15));
        options.MaxRetryAttempts.ShouldBe(10);
        options.PublishedMessageTtlSeconds.ShouldBe(1800);
        options.BatchSize.ShouldBe(200);
        options.Enabled.ShouldBeFalse();
    }
}
