using Alerts.Application;
using Alerts.Domain;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Alerts.Infrastructure.Persistence;

public sealed class AlertRepository(AlertsDbContext dbContext) : IAlertRepository
{
    public async Task<bool> TryAddAsync(Alert alert, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(alert);

        var alreadyExists = await dbContext.Alerts
            .AsNoTracking()
            .AnyAsync(a => a.EventId == alert.EventId, cancellationToken);

        if (alreadyExists)
        {
            return false;
        }

        dbContext.Alerts.Add(alert);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            dbContext.Entry(alert).State = EntityState.Detached;
            return false;
        }
    }

    public async Task<IReadOnlyList<Alert>> ListByUserAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Alerts
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Alert?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.Alerts.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => dbContext.SaveChangesAsync(cancellationToken);

    private static bool IsUniqueViolation(DbUpdateException exception)
        => exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation
        };
}
