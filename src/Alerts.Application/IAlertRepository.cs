using Alerts.Domain;

namespace Alerts.Application;

public interface IAlertRepository
{
    /// <summary>
    /// Persiste una alerta. Devuelve false si el eventId ya existía.
    /// </summary>
    Task<bool> TryAddAsync(Alert alert, CancellationToken cancellationToken);

    Task<IReadOnlyList<Alert>> ListByUserAsync(string userId, CancellationToken cancellationToken);

    Task<Alert?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
