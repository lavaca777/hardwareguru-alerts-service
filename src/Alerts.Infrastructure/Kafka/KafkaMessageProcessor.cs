using Alerts.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Alerts.Infrastructure.Kafka;

public interface IKafkaMessageProcessor
{
    /// <summary>
    /// Procesa un payload. Devuelve true si el offset puede confirmarse.
    /// </summary>
    Task<bool> ProcessAsync(string payload, CancellationToken cancellationToken);
}

public sealed class KafkaMessageProcessor(
    IServiceScopeFactory scopeFactory,
    IOptions<KafkaOptions> options,
    ILogger<KafkaMessageProcessor> logger) : IKafkaMessageProcessor
{
    public async Task<bool> ProcessAsync(
        string payload,
        CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var maxAttempts = Math.Max(1, settings.MaxProcessingRetries);
        var retryDelayMs = Math.Max(0, settings.RetryDelayMs);

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IBuildSavedEventProcessor>();
                var result = await processor.ProcessAsync(payload, cancellationToken);

                logger.LogInformation(
                    "Evento de build procesado con resultado {Result} en el intento {Attempt}.",
                    result,
                    attempt);

                return true;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                if (attempt >= maxAttempts)
                {
                    logger.LogError(
                        exception,
                        "No se pudo procesar el evento tras {Attempts} intentos; no se confirmará el offset.",
                        attempt);

                    return false;
                }

                logger.LogWarning(
                    exception,
                    "Error procesando evento; reintento {Attempt}/{MaxAttempts} en {DelayMs} ms.",
                    attempt,
                    maxAttempts,
                    retryDelayMs);

                await Task.Delay(retryDelayMs, cancellationToken);
            }
        }
    }
}
