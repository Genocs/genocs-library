using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commands;
using Genocs.Core.CQRS.Events;
using Genocs.Core.CQRS.Queries;
using Genocs.Library.Demo.WebApi.Extensions;
using Genocs.Library.Demo.WebApi.Features;
using Genocs.Library.Demo.WebApi.Sagas;
using Genocs.Logging;
using Genocs.Messaging.CQRS;
using Genocs.Messaging.Outbox;
using Genocs.Messaging.Outbox.MongoDB;
using Genocs.Messaging.RabbitMQ;
using Genocs.Persistence.MongoDB.Extensions;
using Genocs.Saga;
using Genocs.Saga.Integrations.Redis;
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
    .AddBookStoreDbContext()
    .AddMongo()
    .AddCommandHandlers()
    .AddEventHandlers()
    .AddQueryHandlers()
    .AddMessageOutbox(o => o.AddMongo());

await gnxBuilder.AddRabbitMQAsync();

gnxBuilder.Build();

// Add services to the container.
var services = builder.Services;

services.AddSaga(x => x.UseRedisPersistence(builder.Configuration, "redis"))
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
    .UseAccessTokenValidator()// Used to validate the access token In RealTime
    .UseRabbitMQ()
    .SubscribeEvent<TransactionCompleted>();

await app.UseBookStoreDbContextAsync();

app.MapControllers();

app.MapFeatures();

await app.RunAsync();

await Log.CloseAndFlushAsync();
