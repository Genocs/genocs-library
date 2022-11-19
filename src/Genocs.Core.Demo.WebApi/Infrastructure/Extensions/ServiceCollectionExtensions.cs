using Genocs.Core.Demo.WebApi.Infrastructure.Extensions;
using Genocs.ServiceBusAzure.Options;
using Genocs.ServiceBusAzure.Queues;
using Genocs.ServiceBusAzure.Queues.Interfaces;
using Genocs.ServiceBusAzure.Topics;
using Genocs.ServiceBusAzure.Topics.Interfaces;
using MassTransit;

namespace Genocs.Core.Demo.WebApi.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAzureServiceBus(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAzureServiceBusTopic(configuration);
        services.AddAzureServiceBusQueue(configuration);

        return services;
    }

    public static IServiceCollection AddAzureServiceBusTopic(this IServiceCollection services, IConfiguration configuration)
    {
        // Register IOptions<TopicSettings>
        services.Configure<TopicSettings>(configuration.GetSection(TopicSettings.Position));

        // HOW to Register TopicSettings instead of IOptions<TopicSettings>
        ////var topicSetting = new TopicOptions();
        ////configuration.GetSection(TopicSettings.Position).Bind(topicSetting);
        ////services.AddSingleton(topicSetting);

        services.AddSingleton<IAzureServiceBusQueue, AzureServiceBusQueue>();

        return services;
    }

    public static IServiceCollection AddAzureServiceBusQueue(this IServiceCollection services, IConfiguration configuration)
    {
        // Register IOptions<QueueSettings>
        services.Configure<QueueSettings>(configuration.GetSection(QueueSettings.Position));

        // HOW to Register QueueSettings instead of IOptions<QueueSettings>
        ////var queueSetting = new QueueSettings();
        ////configuration.GetSection(QueueSettings.Position).Bind(queueSetting);
        ////services.AddSingleton(queueSetting);

        services.AddSingleton<IAzureServiceBusTopic, AzureServiceBusTopic>();

        return services;
    }

    public static IServiceCollection AddMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        var massTransitSetting = new MassTransitSettings();
        configuration.GetSection(MassTransitSettings.Position).Bind(massTransitSetting);

        services.AddSingleton(massTransitSetting);

        services.AddMassTransit(x =>
        {
            //x.AddConsumersFromNamespaceContaining<MerchantStatusChangedEvent>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
                //cfg.UseHealthCheck(context);
                cfg.Host(massTransitSetting.HostName, massTransitSetting.VirtualHost,
                    h =>
                    {
                        h.Username(massTransitSetting.UserName);
                        h.Password(massTransitSetting.Password);
                    }
                );
            });
        });

        return services;
    }
}
