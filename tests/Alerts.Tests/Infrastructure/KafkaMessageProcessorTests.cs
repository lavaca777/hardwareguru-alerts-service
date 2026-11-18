using Alerts.Application;
using Alerts.Infrastructure.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Alerts.Tests.Infrastructure;

public sealed class KafkaMessageProcessorTests
{
    [Fact]
    public async Task ProcessAsync_ReturnsTrue_WhenMessageIsProcessed()
    {
        var processor = new Mock<IBuildSavedEventProcessor>();
        processor
            .Setup(p => p.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(AlertProcessingResult.Created);

        var result = await CreateSut(processor.Object).ProcessAsync(
            "payload",
            CancellationToken.None);

        Assert.True(result);
        processor.Verify(
            p => p.ProcessAsync("payload", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_RetriesAndReturnsTrue_WhenTransientFailureThenSuccess()
    {
        var processor = new Mock<IBuildSavedEventProcessor>();
        processor
            .SetupSequence(p => p.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Transient failure"))
            .ReturnsAsync(AlertProcessingResult.Created);

        var result = await CreateSut(processor.Object).ProcessAsync(
            "payload",
            CancellationToken.None);

        Assert.True(result);
        processor.Verify(
            p => p.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }

    [Fact]
    public async Task ProcessAsync_ReturnsFalse_AfterMaxRetries()
    {
        var processor = new Mock<IBuildSavedEventProcessor>();
        processor
            .Setup(p => p.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Persistent failure"));

        var result = await CreateSut(processor.Object, maxRetries: 3).ProcessAsync(
            "payload",
            CancellationToken.None);

        Assert.False(result);
        processor.Verify(
            p => p.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task ProcessAsync_PropagatesCancellation()
    {
        var processor = new Mock<IBuildSavedEventProcessor>();
        processor
            .Setup(p => p.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => CreateSut(processor.Object).ProcessAsync(
                "payload",
                cancellationTokenSource.Token));

        processor.Verify(
            p => p.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static KafkaMessageProcessor CreateSut(
        IBuildSavedEventProcessor processor,
        int maxRetries = 3)
    {
        var services = new ServiceCollection();
        services.AddSingleton(processor);

        var provider = services.BuildServiceProvider();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        return new KafkaMessageProcessor(
            scopeFactory,
            Options.Create(new KafkaOptions
            {
                MaxProcessingRetries = maxRetries,
                RetryDelayMs = 0
            }),
            NullLogger<KafkaMessageProcessor>.Instance);
    }
}
