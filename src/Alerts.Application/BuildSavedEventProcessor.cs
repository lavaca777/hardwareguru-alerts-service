using System.Text.Json;
using Alerts.Domain;

namespace Alerts.Application;

public interface IBuildSavedEventProcessor
{
    Task<AlertProcessingResult> ProcessAsync(
        string payload,
        CancellationToken cancellationToken);
}

public sealed class BuildSavedEventProcessor(IAlertService alertService) : IBuildSavedEventProcessor
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<AlertProcessingResult> ProcessAsync(
        string payload,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return AlertProcessingResult.Invalid;
        }

        BuildSavedEventMessage? message;

        try
        {
            message = JsonSerializer.Deserialize<BuildSavedEventMessage>(
                payload,
                SerializerOptions);
        }
        catch (JsonException)
        {
            return AlertProcessingResult.Invalid;
        }

        if (message is null)
        {
            return AlertProcessingResult.Invalid;
        }

        return await alertService.ProcessAsync(
            message.ToDomain(),
            cancellationToken);
    }
}
