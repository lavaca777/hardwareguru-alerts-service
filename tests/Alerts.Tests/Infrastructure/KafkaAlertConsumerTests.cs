using Alerts.Infrastructure.Kafka;
using Confluent.Kafka;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Alerts.Tests.Infrastructure;

public sealed class KafkaAlertConsumerTests
{
    [Fact]
    public async Task StartAsync_DoesNotConnect_WhenKafkaIsDisabled()
    {
        var messageProcessor = new Mock<IKafkaMessageProcessor>();
        var consumerFactory = new Mock<IKafkaConsumerFactory>();
        var consumer = new KafkaAlertConsumer(
            Options.Create(new KafkaOptions { Enabled = false }),
            messageProcessor.Object,
            consumerFactory.Object,
            NullLogger<KafkaAlertConsumer>.Instance);

        await consumer.StartAsync(CancellationToken.None);
        await consumer.StopAsync(CancellationToken.None);

        messageProcessor.Verify(
            p => p.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task ConsumeAsync_CommitsOffset_AfterSuccessfulProcessing()
    {
        var result = CreateConsumeResult();
        var cancellationTokenSource = new CancellationTokenSource();
        var consumeCalls = 0;

        var messageProcessor = new Mock<IKafkaMessageProcessor>();
        messageProcessor
            .Setup(p => p.ProcessAsync("payload", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var consumer = new Mock<IConsumer<string, string>>();
        consumer
            .Setup(c => c.Consume(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                consumeCalls++;

                if (consumeCalls == 1)
                {
                    return result;
                }

                cancellationTokenSource.Cancel();
                throw new OperationCanceledException(cancellationTokenSource.Token);
            });

        var consumerFactory = new Mock<IKafkaConsumerFactory>();
        consumerFactory
            .Setup(f => f.Create(It.IsAny<ConsumerConfig>()))
            .Returns(consumer.Object);

        var sut = CreateSut(messageProcessor.Object, consumerFactory.Object);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => sut.ConsumeAsync(
                new KafkaOptions { ReconnectDelayMs = 0, RetryDelayMs = 0 },
                cancellationTokenSource.Token));

        messageProcessor.Verify(
            p => p.ProcessAsync("payload", It.IsAny<CancellationToken>()),
            Times.Once);
        consumer.Verify(c => c.Commit(result), Times.Once);
        consumer.Verify(c => c.Close(), Times.Once);
    }

    [Fact]
    public async Task ConsumeAsync_ContinuesAfterConsumeException()
    {
        var result = CreateConsumeResult();
        var cancellationTokenSource = new CancellationTokenSource();
        var consumeCalls = 0;

        var messageProcessor = new Mock<IKafkaMessageProcessor>();
        messageProcessor
            .Setup(p => p.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var consumer = new Mock<IConsumer<string, string>>();
        consumer
            .Setup(c => c.Consume(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                consumeCalls++;

                if (consumeCalls == 1)
                {
                    throw new ConsumeException(
                        new ConsumeResult<byte[], byte[]>(),
                        new Error(ErrorCode.UnknownTopicOrPart));
                }

                if (consumeCalls == 2)
                {
                    return result;
                }

                cancellationTokenSource.Cancel();
                throw new OperationCanceledException(cancellationTokenSource.Token);
            });

        var consumerFactory = new Mock<IKafkaConsumerFactory>();
        consumerFactory
            .Setup(f => f.Create(It.IsAny<ConsumerConfig>()))
            .Returns(consumer.Object);

        var sut = CreateSut(messageProcessor.Object, consumerFactory.Object);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => sut.ConsumeAsync(
                new KafkaOptions { ReconnectDelayMs = 0, RetryDelayMs = 0 },
                cancellationTokenSource.Token));

        consumer.Verify(c => c.Commit(result), Times.Once);
        consumer.Verify(c => c.Close(), Times.Once);
    }

    [Fact]
    public async Task ConsumeAsync_DoesNotCommitAndSeeksBack_WhenProcessingFails()
    {
        var result = CreateConsumeResult();
        var cancellationTokenSource = new CancellationTokenSource();
        var consumeCalls = 0;

        var messageProcessor = new Mock<IKafkaMessageProcessor>();
        messageProcessor
            .Setup(p => p.ProcessAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var consumer = new Mock<IConsumer<string, string>>();
        consumer
            .Setup(c => c.Consume(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                consumeCalls++;

                if (consumeCalls == 1)
                {
                    return result;
                }

                cancellationTokenSource.Cancel();
                throw new OperationCanceledException(cancellationTokenSource.Token);
            });

        var consumerFactory = new Mock<IKafkaConsumerFactory>();
        consumerFactory
            .Setup(f => f.Create(It.IsAny<ConsumerConfig>()))
            .Returns(consumer.Object);

        var sut = CreateSut(messageProcessor.Object, consumerFactory.Object);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => sut.ConsumeAsync(
                new KafkaOptions { ReconnectDelayMs = 0, RetryDelayMs = 0 },
                cancellationTokenSource.Token));

        consumer.Verify(c => c.Commit(It.IsAny<ConsumeResult<string, string>>()), Times.Never);
        consumer.Verify(c => c.Seek(result.TopicPartitionOffset), Times.Once);
        consumer.Verify(c => c.Close(), Times.Once);
    }

    private static KafkaAlertConsumer CreateSut(
        IKafkaMessageProcessor messageProcessor,
        IKafkaConsumerFactory consumerFactory)
        => new(
            Options.Create(new KafkaOptions()),
            messageProcessor,
            consumerFactory,
            NullLogger<KafkaAlertConsumer>.Instance);

    private static ConsumeResult<string, string> CreateConsumeResult()
        => new()
        {
            Topic = "builds_guardadas",
            Partition = new Partition(0),
            Offset = new Offset(42),
            Message = new Message<string, string>
            {
                Key = "event-key",
                Value = "payload"
            }
        };
}
