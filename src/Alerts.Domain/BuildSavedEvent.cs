namespace Alerts.Domain;

/// <summary>
/// Evento publicado cuando una build se guarda correctamente.
/// Los nombres internos están en inglés; el contrato JSON se mapea en infraestructura.
/// </summary>
public sealed record BuildSavedEvent(
    Guid EventId,
    string UserId,
    string BuildId,
    string BuildName,
    int EstimatedConsumptionW,
    int PsuPowerW,
    DateTimeOffset OccurredAt)
{
    public bool IsValid()
        => EventId != Guid.Empty
           && !string.IsNullOrWhiteSpace(UserId)
           && !string.IsNullOrWhiteSpace(BuildId)
           && !string.IsNullOrWhiteSpace(BuildName)
           && EstimatedConsumptionW >= 0
           && PsuPowerW > 0
           && OccurredAt != default;
}
