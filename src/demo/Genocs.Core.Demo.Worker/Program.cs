using Genocs.Core.Builders;
using Genocs.Core.Demo.Worker;
using Genocs.Core.Demo.Worker.Consumers;
using Genocs.Logging;
using Genocs.Persistence.MongoDb.Extensions;
using Genocs.Tracing;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;

StaticLogger.EnsureInitialized();

IHost host = Host.CreateDefaultBuilder(args)
    .UseLogging()
    .ConfigureServices((hostContext, services) =>
    {
        // Run the hosted service
        services.AddHostedService<MassTransitConsoleHostedService>();

        services
            .AddGenocs(hostContext.Configuration)
            .AddOpenTelemetry()
            .AddMongoWithRegistration();

        ConfigureMassTransit(services, hostContext.Configuration);

    })
    .Build();

await host.RunAsync();

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
    // configurator.UseMessageData(new MongoDbMessageDataRepository("mongodb://127.0.0.1", "attachments"));

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


