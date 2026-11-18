using Alerts.Domain;
using Alerts.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Alerts.Tests.Api;

public sealed class AlertsApiFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = $"alerts-api-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AlertsDbContext>>();
            services.RemoveAll<AlertsDbContext>();

            services.AddDbContext<AlertsDbContext>(
                options => options.UseInMemoryDatabase(databaseName));
        });
    }

    public async Task SeedAsync(params Alert[] alerts)
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AlertsDbContext>();

        dbContext.Alerts.AddRange(alerts);
        await dbContext.SaveChangesAsync();
    }
}
