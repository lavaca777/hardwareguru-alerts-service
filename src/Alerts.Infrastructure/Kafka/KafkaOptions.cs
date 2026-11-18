namespace Alerts.Infrastructure.Kafka;

public sealed class KafkaOptions
{
    public const string SectionName = "Kafka";

    public bool Enabled { get; set; }

    public string BootstrapServers { get; set; } = "127.0.0.1:9092";

    public string Topic { get; set; } = "builds_guardadas";

    public string GroupId { get; set; } = "alerts-service";

    public bool AllowAutoCreateTopics { get; set; }

    public int MaxProcessingRetries { get; set; } = 3;

    public int RetryDelayMs { get; set; } = 1_000;

    public int ReconnectDelayMs { get; set; } = 5_000;
}
