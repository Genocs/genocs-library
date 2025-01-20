using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commands;
using Genocs.Core.CQRS.Events;
using Genocs.Core.CQRS.Queries;
using Genocs.Logging;
using Genocs.MessageBrokers.Outbox;
using Genocs.MessageBrokers.Outbox.MongoDB;
using Genocs.MessageBrokers.RabbitMQ;
using Genocs.Metrics.AppMetrics;
using Genocs.Persistence.MongoDb.Extensions;
using Genocs.Secrets.Vault;
using Genocs.SignalR.WebApi.Commands;
using Genocs.SignalR.WebApi.Exceptions;
using Genocs.SignalR.WebApi.Framework;
using Genocs.SignalR.WebApi.Hubs;
using Genocs.SignalR.WebApi.Services;
using Genocs.Tracing;
using Genocs.WebApi;
using Genocs.WebApi.CQRS;
using Genocs.WebApi.Swagger;
using Genocs.WebApi.Swagger.Docs;
using Serilog;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

builder.Host
        .UseLogging()
        .UseVault();

IGenocsBuilder gnxBuilder = await builder
                                        .AddGenocs()
                                        .AddOpenTelemetry()
                                        .AddMetrics()
                                        .AddJwt()
                                        .AddCorrelationContextLogging()
                                        .AddErrorHandler<ExceptionToResponseMapper>()
                                        .AddMongo()
                                        .AddCommandHandlers()
                                        .AddEventHandlers()
                                        .AddQueryHandlers()
                                        .AddInMemoryCommandDispatcher()
                                        .AddInMemoryEventDispatcher()
                                        .AddInMemoryQueryDispatcher()
                                        .AddMessageOutbox(o => o.AddMongo())
                                        .AddWebApi()
                                        .AddSwaggerDocs()
                                        .AddWebApiSwaggerDocs()
                                        .AddRabbitMQAsync();

var services = builder.Services;
services.AddSignalR();
services.AddTransient<IHubWrapper, HubWrapper>();
services.AddTransient<IHubService, HubService>();

gnxBuilder.Build();

var app = builder.Build();

app.UseGenocs()
    .UserCorrelationContextLogging()
    .UseErrorHandler()
    .UseRouting()
    .UseEndpoints(r =>
    {
        r.MapControllers();
        r.MapHub<GenocsHub>("/notificationHub");
    })
    .UseDispatcherEndpoints(endpoints => endpoints
        .Post<PublishNotification>("notifications", afterDispatch: (cmd, ctx) => ctx.Response.Created($"notifications/{cmd.NotificationId}")))
    .UseSwaggerDocs()
    .UseRabbitMQ();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();
app.MapStaticAssets();

app.Run();

Log.CloseAndFlush();
