using Genocs.Core.Demo.Contracts;
using Genocs.Core.Demo.Worker;
using Genocs.Core.Demo.Worker.Consumers;
using Genocs.Core.Demo.Worker.Handlers;
using Genocs.Core.Interfaces;
using Genocs.Monitoring;
using Genocs.Persistence.MongoDb.Extensions;
using Genocs.ServiceBusAzure.Options;
using Genocs.ServiceBusAzure.Queues;
using Genocs.ServiceBusAzure.Queues.Interfaces;
using Genocs.ServiceBusAzure.Topics;
using Genocs.ServiceBusAzure.Topics.Interfaces;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Serilog;
using Serilog.Events;
using System.Reflection;

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
        TelemetryAndLogging.Initialize(hostContext.Configuration.GetConnectionString("ApplicationInsights"));
        services.AddCustomOpenTelemetry(hostContext.Configuration);


        // It adds the MongoDb Repository to the project and register all the Domain Objects with the standard interface
        services.AddMongoDatabase(hostContext.Configuration);

        // It registers the repositories that has been overridden
        // No need in whenever only 
        services.RegisterRepositories(Assembly.GetExecutingAssembly());

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

        // Providing a connection string is required if you're using the
        // standalone Microsoft.Extensions.Logging.ApplicationInsights package,
        // or when you need to capture logs during application startup, such as
        // in Program.cs or Startup.cs itself.
        logging.AddApplicationInsights(
            configureTelemetryConfiguration: (config) => config.ConnectionString = hostingContext.Configuration.GetConnectionString("ApplicationInsights"),
            configureApplicationInsightsLoggerOptions: (options) =>
            {
            }
        );

        // Capture all log-level entries from Program
        logging.AddFilter<ApplicationInsightsLoggerProvider>(
            typeof(Program).FullName, LogLevel.Trace);

        //// Capture all log-level entries from Startup
        //logging.AddFilter<ApplicationInsightsLoggerProvider>(
        //    typeof(Startup).FullName, LogLevel.Trace);
    })
    .Build();

await host.RunAsync();

await TelemetryAndLogging.FlushAndCloseAsync();

Log.CloseAndFlush();


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
    services.Configure<AzureServiceBusTopicSettings>(configuration.GetSection(AzureServiceBusTopicSettings.Position));

    services.AddSingleton<IAzureServiceBusTopic, AzureServiceBusTopic>();

    services.AddScoped<IEventHandler<DemoEvent>, DemoEventHandler>();

    var topicBus = services.BuildServiceProvider().GetRequiredService<IAzureServiceBusTopic>();
    topicBus.Subscribe<DemoEvent, IEventHandler<DemoEvent>>();

}

static void ConfigureAzureServiceBusQueue(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<AzureServiceBusQueueSettings>(configuration.GetSection(AzureServiceBusQueueSettings.Position));

    services.AddSingleton<IAzureServiceBusQueue, AzureServiceBusQueue>();

    services.AddScoped<ICommandHandler<DemoCommand>, DemoCommandHandler>();

    var queueBus = services.BuildServiceProvider().GetRequiredService<IAzureServiceBusQueue>();
    queueBus.Consume<DemoCommand, ICommandHandler<DemoCommand>>();
}
