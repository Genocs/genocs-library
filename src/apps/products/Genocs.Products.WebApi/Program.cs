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
using Genocs.Metrics.Prometheus;
using Genocs.Persistence.MongoDb.Extensions;
using Genocs.Persistence.Redis;
using Genocs.Products.WebApi;
using Genocs.Products.WebApi.Commands;
using Genocs.Products.WebApi.Domain;
using Genocs.Products.WebApi.DTO;
using Genocs.Products.WebApi.Queries;
using Genocs.Secrets.Vault;
using Genocs.Tracing.Jaeger;
using Genocs.Tracing.Jaeger.RabbitMQ;
using Genocs.WebApi;
using Genocs.WebApi.CQRS;
using Genocs.WebApi.Security;
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

services.AddGenocs()
        .AddErrorHandler<ExceptionToResponseMapper>()
        .AddServices()
        .AddHttpClient()
        .AddCorrelationContextLogging()
        .AddConsul()
        .AddFabio()
        .AddJaeger()
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
        .AddRabbitMq(plugins: p => p.AddJaegerRabbitMqPlugin())
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
        .Get("", ctx => ctx.Response.WriteAsync("Products Service"))
        .Get("ping", ctx => ctx.Response.WriteAsync("pong"))
        .Get<BrowseProducts, PagedResult<ProductDto>>("products")
        .Get<GetProduct, ProductDto>("products/{productId}")
        .Post<CreateProduct>("products",
            afterDispatch: (cmd, ctx) => ctx.Response.Created($"products/{cmd.ProductId}")))
    .UseJaeger()
    .UseSwaggerDocs()
    .UseRabbitMq();

app.Run();

Log.CloseAndFlush();

