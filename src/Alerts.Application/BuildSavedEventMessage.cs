using System.Text.Json.Serialization;
using Alerts.Domain;

namespace Alerts.Application;

/// <summary>
/// Representación JSON del evento BuildGuardada.
/// Los nombres de propiedades respetan el contrato del tópico.
/// </summary>
public sealed class BuildSavedEventMessage
{
    [JsonPropertyName("eventId")]
    public Guid EventId { get; init; }

    [JsonPropertyName("userId")]
    public string? UserId { get; init; }

    [JsonPropertyName("buildId")]
    public string? BuildId { get; init; }

    [JsonPropertyName("buildName")]
    public string? BuildName { get; init; }

    [JsonPropertyName("consumoEstimadoW")]
    public int EstimatedConsumptionW { get; init; }

    [JsonPropertyName("potenciaFuenteW")]
    public int PsuPowerW { get; init; }

    [JsonPropertyName("ocurridoEn")]
    public DateTimeOffset OccurredAt { get; init; }

    public BuildSavedEvent ToDomain()
        => new(
            EventId,
            UserId ?? string.Empty,
            BuildId ?? string.Empty,
            BuildName ?? string.Empty,
            EstimatedConsumptionW,
            PsuPowerW,
            OccurredAt);
}
