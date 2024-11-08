using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commands;
using Genocs.Core.CQRS.Events;
using Genocs.Core.Demo.Contracts;
using Genocs.Core.Demo.Domain.Aggregates;
using Genocs.Core.Demo.Worker;
using Genocs.Core.Demo.Worker.Consumers;
using Genocs.Core.Demo.Worker.Handlers;
using Genocs.Logging;
using Genocs.Persistence.MongoDb.Domain.Repositories;
using Genocs.Persistence.MongoDb.Extensions;
using Genocs.Persistence.MongoDb.Repositories;
using Genocs.ServiceBusAzure.Configurations;
using Genocs.ServiceBusAzure.Queues;
using Genocs.ServiceBusAzure.Queues.Interfaces;
using Genocs.ServiceBusAzure.Topics;
using Genocs.ServiceBusAzure.Topics.Interfaces;
using Genocs.Tracing;
using MassTransit;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;
using System.Reflection;

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
            .AddMongoFast()
            .RegisterMongoRepositories(Assembly.GetExecutingAssembly()); // It registers the repositories that has been overridden. No need in case of standard repository

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

static IServiceCollection RegisterCustomMongoRepository(IServiceCollection services, IConfiguration configuration)
{
    services.AddScoped<IMongoDbRepository<User>, MongoDbRepository<User>>();
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

static void ConfigureAzureServiceBusTopic(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<AzureServiceBusTopicOptions>(configuration.GetSection(AzureServiceBusTopicOptions.Position));

    services.AddSingleton<IAzureServiceBusTopic, AzureServiceBusTopic>();

    services.AddScoped<IEventHandler<DemoEvent>, DemoEventHandler>();

    var topicBus = services.BuildServiceProvider().GetRequiredService<IAzureServiceBusTopic>();
    topicBus.Subscribe<DemoEvent, IEventHandlerLegacy<DemoEvent>>();
}

static void ConfigureAzureServiceBusQueue(IServiceCollection services, IConfiguration configuration)
{
    services.Configure<AzureServiceBusQueueOptions>(configuration.GetSection(AzureServiceBusQueueOptions.Position));

    services.AddSingleton<IAzureServiceBusQueue, AzureServiceBusQueue>();

    services.AddScoped<ICommandHandler<DemoCommand>, DemoCommandHandler>();

    var queueBus = services.BuildServiceProvider().GetRequiredService<IAzureServiceBusQueue>();
    queueBus.Consume<DemoCommand, ICommandHandlerLegacy<DemoCommand>>();
}
