using Genocs.Core.Demo.ServiceBusAzure.Service.Handlers;
using Genocs.Core.Demo.ServiceBusAzure.Worker;
using Genocs.Core.Interfaces;
using Genocs.ServiceBusAzure.Topics;
using Genocs.ServiceBusAzure.Topics.Interfaces;
using MassTransit;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;



DependencyTrackingTelemetryModule _module;
TelemetryClient _telemetryClient;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("MassTransit", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();


var builder = Host.CreateDefaultBuilder(args);

//TelemetryAndLogging.Initialize()

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        _module = new DependencyTrackingTelemetryModule();
        _module.IncludeDiagnosticSourceActivities.Add("MassTransit");

        TelemetryConfiguration configuration = TelemetryConfiguration.CreateDefault();
        configuration.InstrumentationKey = "6b4c6c82-3250-4170-97d3-245ee1449278";
        configuration.TelemetryInitializers.Add(new HttpDependenciesParsingTelemetryInitializer());

        _telemetryClient = new TelemetryClient(configuration);

        _module.Initialize(configuration);

        ConfigureMassTransit(hostContext, services);
        ConfigureAzureServiceBusTopic(hostContext, services);
        //ConfigureAzureServiceBusQueue(hostContext, services);

    })
    .ConfigureLogging((hostingContext, logging) =>
    {
        //logging.AddSerilog(dispose: true);
        logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
    })
    .Build();

await host.RunAsync();

Log.CloseAndFlush();



static void ConfigureMassTransit(HostBuilderContext hostContext, IServiceCollection services)
{
    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
    services.AddMassTransit(cfg =>
    {
        // Consumer configuration
        //cfg.AddConsumersFromNamespaceContaining<AllocateInventoryConsumer>();

        // Routing slip configuration

        //cfg.AddSagaStateMachine<AllocationStateMachine, AllocationState>(typeof(AllocateStateMachineDefinition))
        //    .RedisRepository();

        cfg.UsingRabbitMq(ConfigureBus);
    });

    // Not needed with Masstransit 8.x
    //services.AddHostedService<MassTransitConsoleHostedService>();
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
    services.AddScoped<IEventHandler<NewRedeemReqEvent>, NewRedeemReqEventHandler>();

    services.Configure<TopicOptions>(hostContext.Configuration.GetSection("TopicSettings"));

    services.AddSingleton<IAzureServiceBusTopic, AzureServiceBusTopic>();

    var topicBus = services.BuildServiceProvider().GetRequiredService<IAzureServiceBusTopic>();
    topicBus.Subscribe<NewRedeemReqEvent, IEventHandler<NewRedeemReqEvent>>();
}

static void ConfigureAzureServiceBusQueue(HostBuilderContext hostContext, IServiceCollection services)
{
    //services.AddScoped<ICommandHandler<DemoCommand>, DemoCommandHandler>();

    //services.Configure<QueueOptions>(hostContext.Configuration.GetSection("QueueSettings"));

    //services.AddSingleton<IAzureServiceBusQueue, AzureServiceBusQueue>();

    //var queueBus = services.BuildServiceProvider().GetRequiredService<IAzureServiceBusQueue>();
    //queueBus.Consume<DemoCommand, ICommandHandler<DemoCommand>>();
}

/*
namespace Genocs.Core.Demo.ServiceBusAzure.BusWorker
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            logger.LogInformation("BusWorker Demo is starting...");

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            string environment = System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

            return Host.CreateDefaultBuilder(args)
                        .ConfigureHostConfiguration(configHost => configHost.AddEnvironmentVariables())
                        .ConfigureAppConfiguration((context, builder) =>
                        {
                            builder
                                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                                .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true);

                            // Enable the Secret management
                            // Please check out this link to have more info https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-5.0&tabs=windows
                            builder.AddUserSecrets<Program>();

                            var buildConfig = builder.Build();
                            if (buildConfig["CONFIGURATION_FOLDER"] is var configurationFolder && !string.IsNullOrEmpty(configurationFolder))
                            {
                                builder.AddKeyPerFile(Path.Combine(context.HostingEnvironment.ContentRootPath, configurationFolder), false);
                            }
                        })
                        .ConfigureServices((hostContext, services) =>
                        {
                            services.AddScoped<ICommandHandler<DemoCommand>, DemoCommandHandler>();
                            services.AddScoped<IEventHandler<DemoEvent>, DemoSubscription1EventHandler>();

                            services.Configure<QueueOptions>(hostContext.Configuration.GetSection("QueueSettings"));

                            services.AddSingleton<IAzureServiceBusQueue, AzureServiceBusQueue>();

                            var queueBus = services.BuildServiceProvider().GetRequiredService<IAzureServiceBusQueue>();
                            queueBus.Consume<DemoCommand, ICommandHandler<DemoCommand>>();

                            services.Configure<TopicOptions>(hostContext.Configuration.GetSection("TopicSettings"));

                            services.AddSingleton<IAzureServiceBusTopic, AzureServiceBusTopic>();

                            var topicBus = services.BuildServiceProvider().GetRequiredService<IAzureServiceBusTopic>();
                            topicBus.Subscribe<DemoEvent, IEventHandler<DemoEvent>>();
                        });

        }
    }
}
*/