using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commands;
using Genocs.Core.CQRS.Events;
using Genocs.Core.CQRS.Queries;
using Genocs.Discovery.Consul;
using Genocs.HTTP;
using Genocs.LoadBalancing.Fabio;
using Genocs.Logging;
using Genocs.MessageBrokers.Outbox;
using Genocs.MessageBrokers.Outbox.MongoDB;
using Genocs.MessageBrokers.RabbitMQ;
using Genocs.Metrics.AppMetrics;
using Genocs.Metrics.Prometheus;
using Genocs.Persistence.MongoDb.Extensions;
using Genocs.Persistence.Redis;
using Genocs.Products.WebApi;
using Genocs.Products.WebApi.Commands;
using Genocs.Products.WebApi.Domain;
using Genocs.Products.WebApi.DTO;
using Genocs.Products.WebApi.Queries;
using Genocs.Secrets.Vault;
using Genocs.Tracing;
using Genocs.WebApi;
using Genocs.WebApi.CQRS;
using Genocs.WebApi.Security;
using Genocs.WebApi.Swagger;
using Genocs.WebApi.Swagger.Docs;
using Serilog;

StaticLogger.EnsureInitialized();

var builder = WebApplication.CreateBuilder(args);

builder.Host
        .UseLogging()
        .UseVault();

var services = builder.Services;

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeFormattedMessage = true;
    logging.IncludeScopes = true;
});

services.AddGenocs()
        .AddErrorHandler<ExceptionToResponseMapper>()
        .AddServices()
        .AddHttpClient()
        .AddCorrelationContextLogging()
        .AddConsul()
        .AddFabio()
        .AddOpenTelemetry()
        .AddMetrics()
        .AddMongo()
        .AddMongoRepository<Product, Guid>("products")
        .AddCommandHandlers()
        .AddEventHandlers()
        .AddQueryHandlers()
        .AddInMemoryCommandDispatcher()
        .AddInMemoryEventDispatcher()
        .AddInMemoryQueryDispatcher()
        .AddPrometheus()
        .AddRedis()
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
    .UsePrometheus()
    .UseRouting()
    .UseCertificateAuthentication()
    .UseEndpoints(r => r.MapControllers())
    .UseDispatcherEndpoints(endpoints => endpoints
        .Get(string.Empty, ctx => ctx.Response.WriteAsync("Products Service"))
        .Get("ping", ctx => ctx.Response.WriteAsync("pong"))
        .Get<BrowseProducts, PagedResult<ProductDto>>("products")
        .Get<GetProduct, ProductDto>("products/{productId}")
        .Post<CreateProduct>("products", afterDispatch: (cmd, ctx) => ctx.Response.Created($"products/{cmd.ProductId}")))
    .UseSwaggerDocs()
    .UseRabbitMq();

app.Run();

Log.CloseAndFlush();