using Alerts.Application;
using Alerts.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Falta la cadena de conexión. Configura ConnectionStrings__Postgres.");

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddAlertInfrastructure(connectionString);

var app = builder.Build();

app.UseExceptionHandler();

app.MapControllers();

app.Run();

public partial class Program { }
