using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Alerts.Infrastructure.Persistence;

public sealed class AlertsDbContextFactory : IDesignTimeDbContextFactory<AlertsDbContext>
{
    private const string DesignTimeConnectionString =
        "Host=localhost;Port=5436;Database=hardwareguru_alerts;Username=postgres;Password=postgres";

    public AlertsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AlertsDbContext>()
            .UseNpgsql(DesignTimeConnectionString)
            .Options;

        return new AlertsDbContext(options);
    }
}
