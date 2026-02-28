using Genocs.Core.Builders;
using Genocs.Library.Demo.Masstransit.Worker.Consumers;
using Genocs.Logging;
using Genocs.Messaging.RabbitMQ;
using Genocs.Persistence.MongoDB.Extensions;
using Genocs.Telemetry;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;

StaticLogger.EnsureInitialized();

IHost host = Host.CreateDefaultBuilder(args)
    .UseLogging()
    .ConfigureServices((hostContext, services) =>
    {
        services
            .AddGenocs(hostContext.Configuration)
            .AddTelemetry()
            .AddMongoWithRegistration();

        ConfigureMassTransit(services, hostContext.Configuration);

    })
    .Build();

await host.RunAsync();

await Log.CloseAndFlushAsync();

static IServiceCollection ConfigureMassTransit(IServiceCollection services, IConfiguration configuration)
{
    var rabbitMQSettings = new RabbitMQOptions();
    configuration.GetSection(RabbitMQOptions.Position).Bind(rabbitMQSettings);

    services.TryAddSingleton(KebabCaseEndpointNameFormatter.Instance);
    services.AddMassTransit(cfg =>
    {
        // Consumer configuration
        cfg.AddConsumersFromNamespaceContaining<SubmitOrderConsumer>();

        // Set the transport
        cfg.UsingRabbitMq((context, cfg) =>
        {
            cfg.ConfigureEndpoints(context);

            // cfg.UseHealthCheck(context);
            cfg.Host(
                        rabbitMQSettings.HostNames!.First(),
                        rabbitMQSettings.VirtualHost,
                        h =>
                        {
                            h.Username(rabbitMQSettings.Username);
                            h.Password(rabbitMQSettings.Password);
                        });
        });
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


