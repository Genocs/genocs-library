using System.Text.Json.Serialization;
using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.Core.Demo.WebApi.Infrastructure.Extensions;
using Genocs.Logging;
using Genocs.Metrics.AppMetrics;
using Genocs.Metrics.Prometheus;
using Genocs.Persistence.MongoDb.Extensions;
using Genocs.Secrets.AzureKeyVault;
using Genocs.Tracing;
using Genocs.WebApi;
using Genocs.WebApi.Swagger;
using Genocs.WebApi.Swagger.Docs;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

// using Genocs.Persistence.EFCore.Extensions;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

builder.Host
        .UseAzureKeyVault()
        .UseLogging();

builder
    .AddGenocs()
    // Comment out AddOpenIdJwt since we're using custom Firebase auth
    // .AddOpenIdJwt()
    .AddOpenTelemetry()
    .AddMongoWithRegistration()
    .AddMetrics()
    .AddPrometheus()
    //.AddEFCorePersistence()
    .AddApplicationServices()
    .AddWebApi()
    .AddSwaggerDocs()
    .AddWebApiSwaggerDocs()
    .Build();

var services = builder.Services;

services.AddControllers().AddJsonOptions(x =>
{
    // serialize Enums as strings in api responses (e.g. Role)
    x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

services.AddCors();

services.Configure<HealthCheckPublisherOptions>(options =>
{
    options.Delay = TimeSpan.FromSeconds(2);
    options.Predicate = check => check.Tags.Contains("ready");
});

// Add Masstransit bus configuration
services.AddCustomMassTransit(builder.Configuration);

// Add Firebase authorization configuration
services.AddFirebaseAuthorization(builder.Configuration);

var app = builder.Build();

// Use it only  in case you are using EF Core
//await app.Services.InitializeDatabasesAsync();

app.UseGenocs()
    .UseSwaggerDocs();

app.UseHttpsRedirection();

// global cors policy
app.UseCors(x => x
    .SetIsOriginAllowed(origin => true)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials());

app.UseRouting();

// IMPORTANT: Use your custom Firebase authorization middleware BEFORE UseAuthentication and UseAuthorization
app.UseFirebaseAuthorization();

app.UseAuthentication();
app.UseAuthorization();

app.UseMetrics()
    .UsePrometheus();

app.MapControllers();

app.Run();

Log.CloseAndFlush();
