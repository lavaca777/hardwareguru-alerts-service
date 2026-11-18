using Alerts.Domain;

namespace Alerts.Application;

public sealed class AlertService(IAlertRepository repository) : IAlertService
{
    public async Task<AlertProcessingResult> ProcessAsync(
        BuildSavedEvent buildEvent,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(buildEvent);

        if (!buildEvent.IsValid())
        {
            return AlertProcessingResult.Invalid;
        }

        if (!AlertPolicy.ShouldAlert(buildEvent))
        {
            return AlertProcessingResult.NoAlert;
        }

        var usagePercentage = AlertPolicy.CalculateUsagePercentage(
            buildEvent.EstimatedConsumptionW,
            buildEvent.PsuPowerW);

        var alert = Alert.Create(buildEvent, usagePercentage, DateTimeOffset.UtcNow);

        var added = await repository.TryAddAsync(alert, cancellationToken);

        return added
            ? AlertProcessingResult.Created
            : AlertProcessingResult.Duplicate;
    }

    public Task<IReadOnlyList<Alert>> ListAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("El userId es requerido.", nameof(userId));
        }

        return repository.ListByUserAsync(userId, cancellationToken);
    }

    public async Task<Alert?> MarkAsReadAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        var alert = await repository.GetByIdAsync(id, cancellationToken);

        if (alert is null)
        {
            return null;
        }

        if (!alert.IsRead)
        {
            alert.MarkAsRead();
            await repository.SaveChangesAsync(cancellationToken);
        }

        return alert;
    }
}
