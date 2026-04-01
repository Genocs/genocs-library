using Genocs.Common.CQRS.Queries;
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commands;
using Genocs.Core.CQRS.Events;
using Genocs.Core.CQRS.Queries;
using Genocs.Http;
using Genocs.LoadBalancing.Fabio;
using Genocs.Logging;
using Genocs.Messaging.Outbox;
using Genocs.Messaging.Outbox.MongoDB;
using Genocs.Messaging.RabbitMQ;
using Genocs.Metrics.Prometheus;
using Genocs.Persistence.MongoDB.Extensions;
using Genocs.Persistence.Redis;
using Genocs.Products.WebApi;
using Genocs.Products.WebApi.Commands;
using Genocs.Products.WebApi.Domain;
using Genocs.Products.WebApi.DTO;
using Genocs.Products.WebApi.Queries;
using Genocs.Secrets.HashicorpKeyVault;
using Genocs.ServiceDiscovery.Consul;
using Genocs.Telemetry;
using Genocs.WebApi;
using Genocs.WebApi.CQRS;
using Genocs.WebApi.OpenApi;
using Genocs.WebApi.Security;
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
                                        .AddMongoRepository<Product, Guid>("products")
                                        .AddCommandHandlers()
                                        .AddEventHandlers()
                                        .AddQueryHandlers()
                                        .AddInMemoryCommandDispatcher()
                                        .AddInMemoryEventDispatcher()
                                        .AddInMemoryQueryDispatcher()
                                        .AddPrometheus()
                                        .AddRedis()
                                        .AddMessageOutbox(o => o.AddMongo())
                                        .AddWebApi()
                                        .AddOpenApiDocs()
                                        .AddRabbitMQAsync();

var app = builder.Build();
gnxBuilder.Build(app.Services);

app.UseGenocs()
    .UseCorrelationContextLogging()
    .UseErrorHandler()
    .UsePrometheus()
    .UseRouting()
    .UseCertificateAuthentication()
    .UseEndpoints(r => r.MapControllers())
    .UseDispatcherEndpoints(endpoints => endpoints
        .Get<BrowseProducts, PagedResult<ProductDto>>("products")
        .Get<GetProduct, ProductDto>("products/{productId}")
        .Post<CreateProduct>("products", afterDispatch: (cmd, ctx) => ctx.Response.Created($"products/{cmd.ProductId}")))
    .UseOpenApiDocs()
    .UseRabbitMQ();

app.MapDefaultEndpoints();

app.Run();

Log.CloseAndFlush();