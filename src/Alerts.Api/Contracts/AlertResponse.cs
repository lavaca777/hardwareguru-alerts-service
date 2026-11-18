using System.Text.Json.Serialization;
using Alerts.Domain;

namespace Alerts.Api.Contracts;

public sealed record AlertResponse(
    [property: JsonPropertyName("id")] Guid Id,
    [property: JsonPropertyName("buildId")] string BuildId,
    [property: JsonPropertyName("buildName")] string BuildName,
    [property: JsonPropertyName("porcentajeUso")] int UsagePercentage,
    [property: JsonPropertyName("mensaje")] string Message,
    [property: JsonPropertyName("sugerencia")] string Suggestion,
    [property: JsonPropertyName("leida")] bool IsRead,
    [property: JsonPropertyName("creadaEn")] DateTimeOffset CreatedAt)
{
    public static AlertResponse From(Alert alert)
        => new(
            alert.Id,
            alert.BuildId,
            alert.BuildName,
            alert.UsagePercentage,
            alert.Message,
            alert.Suggestion,
            alert.IsRead,
            alert.CreatedAt);
}
