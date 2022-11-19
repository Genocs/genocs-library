using Genocs.Core.Demo.Contracts;
using Genocs.Core.Demo.Domain.Aggregates;
using Genocs.Core.Demo.ServiceBusAzure.Service.Consumers;
using Genocs.Core.Demo.ServiceBusAzure.Worker;
using Genocs.Core.Demo.ServiceBusAzure.Worker.Handlers;
using Genocs.Core.Domain.Repositories;
using Genocs.Core.Interfaces;
using Genocs.Persistence.MongoDb;
using Genocs.Persistence.MongoDb.Options;
using Genocs.Persistence.MongoDb.Repositories;
using Genocs.ServiceBusAzure.Options;
using Genocs.ServiceBusAzure.Queues;
using Genocs.ServiceBusAzure.Queues.Interfaces;
using Genocs.ServiceBusAzure.Topics;
using Genocs.ServiceBusAzure.Topics.Interfaces;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using Serilog.Events;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        TelemetryAndLogging.Initialize(hostContext.Configuration.GetConnectionString("ApplicationInsights"));

        ConfigureMongoDb(hostContext, services);
        ConfigureMassTransit(hostContext, services);
        //ConfigureAzureServiceBusTopic(hostContext, services);
        //ConfigureAzureServiceBusQueue(hostContext, services);

    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        //logging.AddSerilog(dispose: true);
        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
    })
    .Build();

await host.RunAsync();

await TelemetryAndLogging.FlushAndCloseAsync();

Log.CloseAndFlush();


static void ConfigureMongoDb(HostBuilderContext hostContext, IServiceCollection services)
{
    services.Configure<DBSettings>(hostContext.Configuration.GetSection(DBSettings.Position));

    services.TryAddSingleton<IMongoDatabaseProvider, MongoDatabaseProvider>();
    services.TryAddSingleton<IRepository<Order, string>, MongoDbRepositoryBase<Order, string>>();

    // Add Repository here
}

static void ConfigureMassTransit(HostBuilderContext hostContext, IServiceCollection services)
{
    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
    services.AddMassTransit(cfg =>
    {
        // Consumer configuration
        cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();

        // Set the transport
        cfg.UsingRabbitMq(ConfigureBus);
    });
}

static void ConfigureBus(IBusRegistrationContext context, IRabbitMqBusFactoryConfigurator configurator)
{
    //configurator.UseMessageData(new MongoDbMessageDataRepository("mongodb://127.0.0.1", "attachments"));

    //configurator.ReceiveEndpoint(KebabCaseEndpointNameFormatter.Instance.Consumer<RoutingSlipBatchEventConsumer>(), e =>
    //{
    //    e.PrefetchCount = 20;

    //    e.Batch<RoutingSlipCompleted>(b =>
    //    {
    //        b.MessageLimit = 10;
    //        b.TimeLimit = TimeSpan.FromSeconds(5);

    //        b.Consumer<RoutingSlipBatchEventConsumer, RoutingSlipCompleted>(context);
    //    });
    //});

    // This configuration allow to handle the Scheduling
    configurator.UseMessageScheduler(new Uri("queue:quartz"));

    // This configuration will configure the Activity Definition
    configurator.ConfigureEndpoints(context);
}

static void ConfigureAzureServiceBusTopic(HostBuilderContext hostContext, IServiceCollection services)
{
    services.AddScoped<IEventHandler<OrderRequest>, OrderRequestEventHandler>();

    services.Configure<TopicSettings>(hostContext.Configuration.GetSection(TopicSettings.Position));

    services.AddSingleton<IAzureServiceBusTopic, AzureServiceBusTopic>();

    var topicBus = services.BuildServiceProvider().GetRequiredService<IAzureServiceBusTopic>();
    topicBus.Subscribe<OrderRequest, IEventHandler<OrderRequest>>();
}

static void ConfigureAzureServiceBusQueue(HostBuilderContext hostContext, IServiceCollection services)
{
    services.Configure<QueueSettings>(hostContext.Configuration.GetSection(QueueSettings.Position));

    services.AddSingleton<IAzureServiceBusQueue, AzureServiceBusQueue>();

    var queueBus = services.BuildServiceProvider().GetRequiredService<IAzureServiceBusQueue>();

    services.AddScoped<ICommandHandler<DemoCommand>, DemoCommandHandler>();
    //queueBus.Consume<DemoCommand, ICommandHandler<DemoCommand>>();
}
