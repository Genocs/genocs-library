using Genocs.Auth;
using Genocs.Library.Demo.WebApi.BookStore.Data;
using Genocs.Core.Builders;
using Genocs.Library.Demo.WebApi.Features;
using Genocs.Library.Demo.WebApi.Services;
using Genocs.Logging;
using Genocs.Saga;
using Genocs.Telemetry;
using Genocs.WebApi;
using Genocs.Library.Demo.WebApi.Securities;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Genocs.WebApi.OpenApi;

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
    .AddOpenApiDocs();

gnxBuilder.Build();

// Add services to the container.
var services = builder.Services;
string bookStoreConnectionString = builder.Configuration.GetConnectionString("BookStore")
    ?? throw new InvalidOperationException("Missing 'ConnectionStrings:BookStore' configuration.");

services
    .AddDbContext<BookStoreDbContext>(options => options.UseSqlServer(bookStoreConnectionString))
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

await BookStoreDatabaseInitializer.InitializeAsync(app.Services);

app.UseGenocs()
    .UseCorrelationContextLogging()
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
