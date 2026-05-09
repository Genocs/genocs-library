using Genocs.Core.Builders;
using Genocs.Core.CQRS.Events;
using Genocs.Library.Demo.Contracts;
using Genocs.Logging;
using Genocs.Messaging;
using Genocs.Messaging.CQRS;
using Genocs.Messaging.RabbitMQ;
using Genocs.Persistence.MongoDB.Extensions;
using Genocs.Telemetry;
using Serilog;

StaticLogger.EnsureInitialized();

IGenocsBuilder? gnxBuilder = null;

IHost host = Host.CreateDefaultBuilder(args)
    .UseLogging()
    .ConfigureServices((hostContext, services) =>
    {
        gnxBuilder = services
            .AddGenocs(hostContext.Configuration)
            .AddTelemetry()
                .AddCorrelationContextLogging()
                .AddEventHandlers()
                .AddMongoWithRegistration();

        gnxBuilder.AddRabbitMQAsync().GetAwaiter().GetResult();
    })
    .Build();

gnxBuilder?.Build(host.Services);

host.Services.GetRequiredService<IBusSubscriber>()
    .SubscribeEvent<TransactionCompleted>();

await host.RunAsync();

await Log.CloseAndFlushAsync();
