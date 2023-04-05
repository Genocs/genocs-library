using Genocs.Auth;
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commands;
using Genocs.Core.CQRS.Events;
using Genocs.Core.CQRS.Queries;
using Genocs.Logging;
using Genocs.Secrets.Vault;
using Genocs.SignalR.WebApi.Exceptions;
using Genocs.SignalR.WebApi.Framework;
using Genocs.SignalR.WebApi.Hubs;
using Genocs.SignalR.WebApi.Services;
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
        //.AddServices()
        //.AddHttpClient()
        //.AddConsul()
        //.AddFabio()
        //.AddJaeger()
        //.AddMongo()
        //.AddMongoRepository<Order, Guid>("orders")
        .AddCommandHandlers()
        .AddEventHandlers()
        .AddQueryHandlers()
        .AddInMemoryCommandDispatcher()
        .AddInMemoryEventDispatcher()
        .AddInMemoryQueryDispatcher()
        //.AddPrometheus()
        //.AddRedis()
        //.AddRabbitMq(plugins: p => p.AddJaegerRabbitMqPlugin())
        //.AddMessageOutbox(o => o.AddMongo())
        .AddWebApi()
        .AddSwaggerDocs()
        .AddWebApiSwaggerDocs()
        .Build();

var app = builder.Build();


app.UseGenocs()
    .UserCorrelationContextLogging()
    .UseErrorHandler()
    //.UseCertificateAuthentication()
    //.UsePrometheus()
    .UseRouting()
    .UseEndpoints(r => {
        r.MapControllers();
        r.MapHub<GenocsHub>("/notificationHub");
    })
    .UseDispatcherEndpoints(endpoints => endpoints
        .Get("", ctx => ctx.Response.WriteAsync("SignalR Service"))
        .Get("ping", ctx => ctx.Response.WriteAsync("pong")))
    //.Get<GetOrder, OrderDto>("orders/{orderId}")
    //.Post<CreateOrder>("orders",
    //    afterDispatch: (cmd, ctx) => ctx.Response.Created($"orders/{cmd.OrderId}")))
    //.UseJaeger()
    .UseSwaggerDocs();
//.UseRabbitMq();
//.SubscribeEvent<DeliveryStarted>();


app.UseHttpsRedirection();
app.UseStaticFiles();

app.Run();

Log.CloseAndFlush();
