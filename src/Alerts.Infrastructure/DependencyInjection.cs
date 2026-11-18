using Alerts.Application;
using Alerts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Alerts.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAlertInfrastructure(
        this IServiceCollection services,
        string connectionString)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

        services.AddDbContext<AlertsDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IAlertRepository, AlertRepository>();

        return services;
    }
}
