using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.Library.Demo.WebApi.Extensions;
using Genocs.Library.Demo.WebApi.Features;
using Genocs.Library.Demo.WebApi.Securities;
using Genocs.Library.Demo.WebApi.Services;
using Genocs.Logging;
using Genocs.Saga;
using Genocs.Telemetry;
using Genocs.WebApi;
using Genocs.WebApi.OpenApi;
using Serilog;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .UseLogging();

IGenocsBuilder gnxBuilder = builder
    .AddGenocs()
    .AddTelemetry()
    .AddJwt("simmetric_jwt")
    .AddCorrelationContextLogging()
    .AddWebApi()
    .AddOpenApiDocs()
    .AddBookStoreDbContext();

gnxBuilder.Build();

// Add services to the container.
var services = builder.Services;

services.AddSaga()
    .AddCors(options =>
    {
        options.AddDefaultPolicy(builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    })
    .AddControllers();

// Registrazione del servizio Saga
services.AddScoped<ISagaTransactionService, SagaTransactionService>();

services.MapSecurityFeatures();

var app = builder.Build();

app.UseGenocs()
    .UseCorrelationContextLogging()
    .UseOpenApiDocs()
    .UseHttpsRedirection()
    .UseCors()
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization()
    .UseAccessTokenValidator(); // Used to validate the access token In RealTime

await app.UseBookStoreDbContextAsync();

app.MapControllers();

app.MapFeatures();

await app.RunAsync();

await Log.CloseAndFlushAsync();
