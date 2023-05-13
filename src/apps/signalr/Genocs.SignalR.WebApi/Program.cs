using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commands;
using Genocs.Core.CQRS.Events;
using Genocs.Core.CQRS.Queries;
using Genocs.Logging;
using Genocs.MessageBrokers.Outbox;
using Genocs.MessageBrokers.Outbox.MongoDB;
using Genocs.MessageBrokers.RabbitMQ;
using Genocs.Persistence.MongoDb.Legacy;
using Genocs.Secrets.Vault;
using Genocs.SignalR.WebApi.Commands;
using Genocs.SignalR.WebApi.Exceptions;
using Genocs.SignalR.WebApi.Framework;
using Genocs.SignalR.WebApi.Hubs;
using Genocs.SignalR.WebApi.Services;
using Genocs.Tracing.Jaeger;
using Genocs.WebApi;
using Genocs.WebApi.CQRS;
using Genocs.WebApi.Swagger;
using Genocs.WebApi.Swagger.Docs;
using Serilog;
using Serilog.Events;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

builder.Host
        .UseLogging()
        .UseVault();

var services = builder.Services;

services.AddSignalR();

services.AddTransient<IHubWrapper, HubWrapper>();
services.AddTransient<IHubService, HubService>();

services.AddGenocs()
        .AddCorrelationContextLogging()
        .AddJwt()
        .AddErrorHandler<ExceptionToResponseMapper>()
        .AddJaeger()
        .AddMongo()
        .AddCommandHandlers()
        .AddEventHandlers()
        .AddQueryHandlers()
        .AddInMemoryCommandDispatcher()
        .AddInMemoryEventDispatcher()
        .AddInMemoryQueryDispatcher()
        .AddRabbitMq()
        .AddMessageOutbox(o => o.AddMongo())
        .AddWebApi()
        .AddSwaggerDocs()
        .AddWebApiSwaggerDocs()
        .Build();

var app = builder.Build();


app.UseGenocs()
    .UserCorrelationContextLogging()
    .UseErrorHandler()
    .UseRouting()
    .UseEndpoints(r => {
        r.MapControllers();
        r.MapHub<GenocsHub>("/notificationHub");
    })
    .UseDispatcherEndpoints(endpoints => endpoints
        .Get("", ctx => ctx.Response.WriteAsync("SignalR Service"))
        .Get("ping", ctx => ctx.Response.WriteAsync("pong"))
        .Post<PublishNotification>("notifications",
            afterDispatch: (cmd, ctx) => ctx.Response.Created($"notifications/{cmd.NotificationId}"))
    )
    .UseJaeger()
    .UseSwaggerDocs()
    .UseRabbitMq();


app.UseHttpsRedirection();
app.UseStaticFiles();

app.Run();

Log.CloseAndFlush();
