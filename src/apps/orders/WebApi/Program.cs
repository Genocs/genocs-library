using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commands;
using Genocs.Core.CQRS.Events;
using Genocs.Core.CQRS.Queries;
using Genocs.Discovery.Consul;
using Genocs.Http;
using Genocs.LoadBalancing.Fabio;
using Genocs.Logging;
using Genocs.Messaging.CQRS;
using Genocs.Messaging.Outbox;
using Genocs.Messaging.Outbox.MongoDB;
using Genocs.Messaging.RabbitMQ;
using Genocs.Metrics.Prometheus;
using Genocs.Orders.WebApi;
using Genocs.Orders.WebApi.Commands;
using Genocs.Orders.WebApi.Domain;
using Genocs.Orders.WebApi.DTO;
using Genocs.Orders.WebApi.Events.External;
using Genocs.Orders.WebApi.Queries;
using Genocs.Persistence.MongoDB.Extensions;
using Genocs.Persistence.Redis;
using Genocs.Secrets.HashicorpKeyVault;
using Genocs.Telemetry;
using Genocs.WebApi;
using Genocs.WebApi.CQRS;
using Genocs.WebApi.Security;
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
                                    .AddHttpClient()
                                    .AddConsul()
                                    .AddFabio()
                                    .AddErrorHandler<ExceptionToResponseMapper>()
                                    .AddServices()
                                    .AddCorrelationContextLogging()
                                    .AddMongo()
                                    .AddMongoRepository<Order, Guid>("orders")
                                    .AddCommandHandlers()
                                    .AddEventHandlers()
                                    .AddQueryHandlers()
                                    .AddInMemoryCommandDispatcher()
                                    .AddInMemoryEventDispatcher()
                                    .AddInMemoryQueryDispatcher()
                                    .AddPrometheus()
                                    .AddRedis()
                                    .AddRabbitMQAsync();

gnxBuilder.AddMessageOutbox(o => o.AddMongo())
        .AddWebApi()
        .AddSwaggerDocs()
        .AddWebApiSwaggerDocs()
        .Build();

var app = builder.Build();

app.UseGenocs()
    .UserCorrelationContextLogging()
    .UseErrorHandler()
    .UsePrometheus()
    .UseRouting()
    .UseCertificateAuthentication()
    .UseEndpoints(r => r.MapControllers())
    .UseDispatcherEndpoints(endpoints => endpoints
        .Get<GetOrder, OrderDto>("orders/{orderId}")
        .Post<CreateOrder>("orders", afterDispatch: (cmd, ctx) => ctx.Response.Created($"orders/{cmd.OrderId}")))
    .UseSwaggerDocs()
    .UseRabbitMQ()
    .SubscribeEvent<DeliveryStarted>();

app.MapDefaultEndpoints();

await app.RunAsync();

Log.CloseAndFlush();