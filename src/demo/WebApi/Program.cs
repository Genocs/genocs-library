using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.Library.Demo.Services;
using Genocs.Library.Demo.WebApi.Features;
using Genocs.Library.Demo.WebApi.Services;
using Genocs.Logging;
using Genocs.Saga;
using Genocs.Telemetry;
using Genocs.WebApi;
using Genocs.WebApi.OpenApi.Docs;
using Genocs.Library.Demo.WebApi.Securities;
using Serilog;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

builder.Host
    .UseLogging();

builder
    .AddGenocs()
    .AddJwt("simmetric_jwt")
    .AddTelemetry()
    .AddWebApi()
    .AddOpenApiDocs()
    .Build();

// Add services to the container.
var services = builder.Services;

services
    .AddSaga()
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
    .UseOpenApiDocs();

app.UseHttpsRedirection();

app.UseCors();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Used to validate the access token
// In RealTime
app.UseAccessTokenValidator();

app.MapControllers();
app.MapFeatures();

await app.RunAsync();

await Log.CloseAndFlushAsync();
