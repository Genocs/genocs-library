using Genocs.Core.Demo.Contracts;
using Genocs.Core.Demo.Domain.Aggregates;
using Genocs.Core.Demo.Worker;
using Genocs.Core.Demo.Worker.Consumers;
using Genocs.Core.Demo.Worker.Handlers;
using Genocs.Core.Domain.Repositories;
using Genocs.Core.Interfaces;
using Genocs.Monitoring;
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
    .ConfigureAppConfiguration((hostContext, builder) =>
    {
        builder.AddUserSecrets<Program>();
    })
    .ConfigureServices((hostContext, services) =>
    {
        //TelemetryAndLogging.Initialize(hostContext.Configuration.GetConnectionString("ApplicationInsights"));
        services.AddCustomOpenTelemetry(hostContext.Configuration);

        ConfigureMongoDb(services, hostContext.Configuration);
        ConfigureMassTransit(services, hostContext.Configuration);
        //ConfigureAzureServiceBusTopic(services, hostContext.Configuration);
        //ConfigureAzureServiceBusQueue(services, hostContext.Configuration);

        // Run the hosted service 
        services.AddHostedService<MassTransitConsoleHostedService>();
    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        logging.AddSerilog(dispose: true);
        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
    })
    .Build();

await host.RunAsync();

await TelemetryAndLogging.FlushAndCloseAsync();

Log.CloseAndFlush();


static IServiceCollection ConfigureMongoDb(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.Position));

    services.TryAddSingleton<IMongoDatabaseProvider, MongoDatabaseProvider>();
    services.TryAddSingleton<IRepository<Order, string>, MongoDbRepositoryBase<Order, string>>();

    // Add Repository here

    return services;
}

static IServiceCollection ConfigureMassTransit(IServiceCollection services, IConfiguration configuration)
{
    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
    services.AddMassTransit(cfg =>
    {
        // Consumer configuration
        cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();

        // Set the transport
        cfg.UsingRabbitMq(ConfigureBus);
    });

    return services;
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

static void ConfigureAzureServiceBusTopic(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<TopicSettings>(configuration.GetSection(TopicSettings.Position));

    services.AddSingleton<IAzureServiceBusTopic, AzureServiceBusTopic>();

    services.AddScoped<IEventHandler<DemoEvent>, DemoEventHandler>();

    var topicBus = services.BuildServiceProvider().GetRequiredService<IAzureServiceBusTopic>();
    topicBus.Subscribe<DemoEvent, IEventHandler<DemoEvent>>();

}

static void ConfigureAzureServiceBusQueue(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<QueueSettings>(configuration.GetSection(QueueSettings.Position));

    services.AddSingleton<IAzureServiceBusQueue, AzureServiceBusQueue>();

    services.AddScoped<ICommandHandler<DemoCommand>, DemoCommandHandler>();

    var queueBus = services.BuildServiceProvider().GetRequiredService<IAzureServiceBusQueue>();
    queueBus.Consume<DemoCommand, ICommandHandler<DemoCommand>>();
}
