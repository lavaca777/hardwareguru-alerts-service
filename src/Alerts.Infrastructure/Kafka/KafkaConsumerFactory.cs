using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace Alerts.Infrastructure.Kafka;

public interface IKafkaConsumerFactory
{
    IConsumer<string, string> Create(ConsumerConfig config);
}

public sealed class KafkaConsumerFactory(ILogger<KafkaConsumerFactory> logger) : IKafkaConsumerFactory
{
    public IConsumer<string, string> Create(ConsumerConfig config)
        => new ConsumerBuilder<string, string>(config)
            .SetLogHandler((_, log) =>
                logger.LogDebug("Kafka: {Message}", log.Message))
            .SetErrorHandler((_, error) =>
                logger.LogError(
                    "Kafka error {Code}: {Reason}",
                    error.Code,
                    error.Reason))
            .Build();
}
