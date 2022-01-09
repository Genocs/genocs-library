using Genocs.Core.Demo.Contracts;
using Genocs.Core.Demo.ServiceBusAzure.BusWorker.Handlers;
using Genocs.Core.Interfaces;
using Genocs.ServiceBusAzure.Queues;
using Genocs.ServiceBusAzure.Queues.Interfaces;
using Genocs.ServiceBusAzure.Topics;
using Genocs.ServiceBusAzure.Topics.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;
using UTU.Platform.Demo.AzureServiceBus.BusWorker.Handlers;

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
