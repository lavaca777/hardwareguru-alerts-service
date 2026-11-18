using Alerts.Application;
using Alerts.Infrastructure;
using Alerts.Infrastructure.Kafka;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException(
        "Falta la cadena de conexión. Configura ConnectionStrings__Postgres.");

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddScoped<IAlertService, AlertService>();
builder.Services.AddAlertInfrastructure(connectionString);

builder.Services.Configure<KafkaOptions>(
    builder.Configuration.GetSection(KafkaOptions.SectionName));
builder.Services.AddScoped<IBuildSavedEventProcessor, BuildSavedEventProcessor>();
builder.Services.AddSingleton<IKafkaMessageProcessor, KafkaMessageProcessor>();
builder.Services.AddSingleton<IKafkaConsumerFactory, KafkaConsumerFactory>();
builder.Services.AddHostedService<KafkaAlertConsumer>();

var app = builder.Build();

app.UseExceptionHandler();

app.MapControllers();

app.Run();

public partial class Program { }
