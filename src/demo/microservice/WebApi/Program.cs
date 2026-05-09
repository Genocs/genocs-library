using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commands;
using Genocs.Core.CQRS.Events;
using Genocs.Core.CQRS.Queries;
using Genocs.Library.Demo.Contracts;
using Genocs.Library.Demo.WebApi.Extensions;
using Genocs.Library.Demo.WebApi.Features;
using Genocs.Logging;
using Genocs.Messaging.CQRS;
using Genocs.Messaging.Outbox;
using Genocs.Messaging.Outbox.MongoDB;
using Genocs.Messaging.RabbitMQ;
using Genocs.Persistence.EFCore.Extensions;
using Genocs.Persistence.EFCore.MultiTenancy;
using Genocs.Persistence.MongoDB.Extensions;
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
    .AddEFCorePersistence()
    .AddBookStoreDbContext()
    .AddApplicationSaga()
    .AddMongoWithRegistration()
    .AddCommandHandlers()
    .AddEventHandlers()
    .AddQueryHandlers()
    .AddMessageOutbox(o => o.AddMongo());

await gnxBuilder.AddRabbitMQAsync();

// Add Finbuckle multitenancy registration
gnxBuilder.AddApplicationMultiTenancy(builder.Configuration);

// Add services to the container.
var services = builder.Services;

services
    .AddCors(x =>
    {
        x.AddDefaultPolicy(builder =>
        {
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    })
    .AddControllers();

services.MapSecurityFeatures();
var app = builder.Build();

gnxBuilder.Build(app.Services);

app.UseGenocs()
    .UseCorrelationContextLogging()
    .UseOpenApiDocs()
    .UseHttpsRedirection()
    .UseCors()
    .UseRouting()
    .UseAuthentication()
    .UseAuthorization()
    .UseAccessTokenValidator()
    .UseRabbitMQ()
    .SubscribeEvent<TransactionCompleted>();

app.UseMultiTenancy();

await app.UseBookStoreDbContextAsync();

app.MapControllers();

app.MapFeatures();

await app.RunAsync();

await Log.CloseAndFlushAsync();
