using Genocs.Core.Demo.WebApi.Infrastructure.Extensions;
using Genocs.ServiceBusAzure.Queues;
using Genocs.ServiceBusAzure.Queues.Interfaces;
using Genocs.ServiceBusAzure.Topics;
using Genocs.ServiceBusAzure.Topics.Interfaces;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Core.Demo.WebApi.Infrastructure.Extensions
{
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
            // Register IOptions<TopicOptions>
            services.Configure<TopicOptions>(configuration.GetSection("TopicSettings"));

            // HOW to Register TopicOptions instead of IOptions<TopicOptions>
            ////var topicSetting = new TopicOptions();
            ////configuration.GetSection("TopicSettings").Bind(topicSetting);
            ////services.AddSingleton(topicSetting);

            services.AddSingleton<IAzureServiceBusQueue, AzureServiceBusQueue>();

            return services;
        }

        public static IServiceCollection AddAzureServiceBusQueue(this IServiceCollection services, IConfiguration configuration)
        {
            // Register IOptions<QueueOptions>
            services.Configure<QueueOptions>(configuration.GetSection("QueueSettings"));

            // HOW to Register TopicOptions instead of IOptions<TopicOptions>
            ////var queueSetting = new QueueOptions();
            ////configuration.GetSection("QueueSettings").Bind(queueSetting);
            ////services.AddSingleton(queueSetting);

            services.AddSingleton<IAzureServiceBusTopic, AzureServiceBusTopic>();

            return services;
        }

        public static IServiceCollection AddMassTransit(this IServiceCollection services, IConfiguration configuration)
        {
            var massTransitSetting = new MassTransitSetting();
            configuration.GetSection("MassTransitSetting").Bind(massTransitSetting);

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
            })
            .AddMassTransitHostedService();

            return services;
        }
    }
}
