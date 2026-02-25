using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commands;
using Genocs.Core.CQRS.Events;
using Genocs.Core.CQRS.Queries;
using Genocs.Logging;
using Genocs.Messaging.Outbox;
using Genocs.Messaging.Outbox.MongoDB;
using Genocs.Messaging.RabbitMQ;
using Genocs.Notifications.WebApi.Commands;
using Genocs.Notifications.WebApi.Exceptions;
using Genocs.Notifications.WebApi.Hubs;
using Genocs.Notifications.WebApi.Services;
using Genocs.Persistence.MongoDB.Extensions;
using Genocs.Secrets.HashicorpKeyVault;
using Genocs.Telemetry;
using Genocs.WebApi;
using Genocs.WebApi.CQRS;
using Genocs.WebApi.OpenApi;
using Genocs.WebApi.OpenApi.Docs;
using Serilog;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

builder.Host
        .UseLogging()
        .UseVault();

IGenocsBuilder gnxBuilder = await builder
                                        .AddGenocs()
                                        .AddTelemetry()
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
