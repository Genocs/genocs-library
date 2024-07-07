using Genocs.Core.Demo.WebApi.Configurations;
using Genocs.ServiceBusAzure.Configurations;
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
        services.Configure<AzureServiceBusTopicOptions>(configuration.GetSection(AzureServiceBusTopicOptions.Position));

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
        services.Configure<AzureServiceBusQueueOptions>(configuration.GetSection(AzureServiceBusQueueOptions.Position));

        // HOW to Register QueueSettings instead of IOptions<QueueSettings>
        ////var queueSetting = new QueueSettings();
        ////configuration.GetSection(QueueSettings.Position).Bind(queueSetting);
        ////services.AddSingleton(queueSetting);

        services.AddSingleton<IAzureServiceBusTopic, AzureServiceBusTopic>();

        return services;
    }

    public static IServiceCollection AddCustomMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        RabbitMQOptions rabbitMQSettings = new RabbitMQOptions();
        configuration.GetSection(RabbitMQOptions.Position).Bind(rabbitMQSettings);

        // This is another way to get the RabbitMQOptions
        // RabbitMQOptions? rabbitMQSettingsV2 = configuration.GetSection(RabbitMQOptions.Position).Get<RabbitMQOptions>();

        services.AddSingleton(rabbitMQSettings);

        services.AddMassTransit(x =>
        {
            //x.AddConsumersFromNamespaceContaining<MerchantStatusChangedEvent>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
                //cfg.UseHealthCheck(context);
                cfg.Host(rabbitMQSettings.HostName, rabbitMQSettings.VirtualHost,
                    h =>
                    {
                        h.Username(rabbitMQSettings.UserName);
                        h.Password(rabbitMQSettings.Password);
                    }
                );
            });
        });

        return services;
    }
}
