using Genocs.Core.Demo.WebApi.Infrastructure.Extensions;
using Genocs.Monitoring;
using Genocs.Persistence.MongoDb.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.Text.Json.Serialization;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);


builder.Host.UseSerilog((ctx, lc) =>
{
    lc.WriteTo.Console();

    string? applicationInsightsConnectionString = builder.Configuration.GetConnectionString("ApplicationInsights");

    if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
    {
        lc.WriteTo.ApplicationInsights(new TelemetryConfiguration
        {
            ConnectionString = applicationInsightsConnectionString
        }, TelemetryConverter.Traces);
    }
});


// add services to DI container
var services = builder.Services;

services.AddMongoDatabase(builder.Configuration);
services.RegisterRepositories(Assembly.GetExecutingAssembly());


//ConfigureAzureServiceBusTopic(services, builder.Configuration);
//ConfigureAzureServiceBusQueue(services, builder.Configuration);

services.AddCors();
services.AddControllers().AddJsonOptions(x =>
{
    // serialize enums as strings in api responses (e.g. Role)
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

services.AddHealthChecks();

services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.FromSeconds(2);
    options.Predicate = check => check.Tags.Contains("ready");
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

// Add Masstransit bus configuration
services.AddCustomMassTransit(builder.Configuration);

// Azure Service bus configuration
//services.AddAzureServiceBusQueue(builder.Configuration);
//services.AddAzureServiceBusTopic(builder.Configuration);

services.AddOptions();

// Set Custom Open telemetry
services.AddCustomOpenTelemetry(builder.Configuration);

var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


// global cors policy
app.UseCors(x => x
    .SetIsOriginAllowed(origin => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());


app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/healthz");

app.Run();

Log.CloseAndFlush();


//static void ConfigureAzureServiceBusTopic(IServiceCollection services, IConfiguration configuration)
//{
//    services.Configure<TopicSettings>(configuration.GetSection(TopicSettings.Position));
//    services.AddSingleton<IAzureServiceBusTopic, AzureServiceBusTopic>();
//}


//static void ConfigureAzureServiceBusQueue(IServiceCollection services, IConfiguration configuration)
//{
//    services.Configure<QueueSettings>(configuration.GetSection(QueueSettings.Position));
//    services.AddSingleton<IAzureServiceBusQueue, AzureServiceBusQueue>();
//}