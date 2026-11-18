using Alerts.Domain;

namespace Alerts.Application;

public interface IAlertService
{
    Task<AlertProcessingResult> ProcessAsync(
        BuildSavedEvent buildEvent,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<Alert>> ListAsync(
        string userId,
        CancellationToken cancellationToken);

    Task<Alert?> MarkAsReadAsync(
        Guid id,
        CancellationToken cancellationToken);
}
