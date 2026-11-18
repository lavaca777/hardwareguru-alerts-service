using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alerts.Infrastructure.Kafka;

public sealed class KafkaAlertConsumer(
    IOptions<KafkaOptions> options,
    IKafkaMessageProcessor messageProcessor,
    IKafkaConsumerFactory consumerFactory,
    ILogger<KafkaAlertConsumer> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var settings = options.Value;

        logger.LogInformation(
            "Consumidor Kafka: Enabled={Enabled}, BootstrapServers={BootstrapServers}, Topic={Topic}, GroupId={GroupId}.",
            settings.Enabled,
            settings.BootstrapServers,
            settings.Topic,
            settings.GroupId);

        if (!settings.Enabled)
        {
            logger.LogInformation("Consumidor Kafka deshabilitado por configuración.");
            return;
        }

        // No bloquear el arranque del host con el primer Consume().
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ConsumeAsync(settings, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "El consumidor Kafka se detuvo; reintentando conexión en {DelayMs} ms.",
                    Math.Max(0, settings.ReconnectDelayMs));

                await Task.Delay(Math.Max(0, settings.ReconnectDelayMs), stoppingToken);
            }
        }
    }

    internal async Task ConsumeAsync(
        KafkaOptions settings,
        CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = settings.BootstrapServers,
            GroupId = settings.GroupId,
            EnableAutoCommit = false,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            AllowAutoCreateTopics = settings.AllowAutoCreateTopics
        };

        logger.LogInformation("Creando consumidor Kafka...");
        using var consumer = consumerFactory.Create(config);
        logger.LogInformation("Consumidor creado; suscribiendo al tópico {Topic}...", settings.Topic);
        consumer.Subscribe(settings.Topic);
        logger.LogInformation("Suscripción completada; esperando mensajes...");

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                ConsumeResult<string, string>? result;

                try
                {
                    result = consumer.Consume(stoppingToken);
                }
                catch (ConsumeException exception)
                {
                    logger.LogWarning(
                        exception,
                        "Error consumiendo de Kafka; el consumidor continuará.");
                    await Task.Delay(Math.Max(0, settings.RetryDelayMs), stoppingToken);
                    continue;
                }

                if (result?.Message?.Value is null)
                {
                    continue;
                }

                var processed = await messageProcessor.ProcessAsync(
                    result.Message.Value,
                    stoppingToken);

                if (!processed)
                {
                    consumer.Seek(result.TopicPartitionOffset);
                    logger.LogWarning(
                        "El evento {Offset} no se procesó; se rewindea para reintentarlo.",
                        result.TopicPartitionOffset);
                    await Task.Delay(Math.Max(0, settings.ReconnectDelayMs), stoppingToken);
                    continue;
                }

                consumer.Commit(result);
                logger.LogInformation(
                    "Offset confirmado: {Offset}.",
                    result.TopicPartitionOffset);
            }
        }
        finally
        {
            try
            {
                consumer.Close();
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Error cerrando el consumidor Kafka.");
            }
        }
    }
}
