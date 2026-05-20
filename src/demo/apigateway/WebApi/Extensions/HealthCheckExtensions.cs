using Genocs.Messaging.RabbitMQ;
using Genocs.Persistence.MongoDB;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MongoDB.Driver;

namespace Genocs.Library.Demo.WebApi.Extensions;

/// <summary>
/// Extension method for health check related services and middlewares.
/// </summary>
public static class HealthCheckExtensions
{
    private static readonly string[] ApiTags = ["readiness", "external"];
    private static readonly string[] MessagingTags = ["readiness", "messaging"];
    private static readonly string[] DatabaseTags = ["readiness", "database"];

    /// <summary>
    /// Adds health check services to the dependency injection container. This method registers various health checks for MongoDB, RabbitMQ, and API clients.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the health checks to.</param>
    /// <returns>The IServiceCollection with the health checks added.</returns>
    public static IServiceCollection AddHealthCheck(this IServiceCollection services)
    {
        var healthChecks = services.AddHealthChecks();

        healthChecks.AddMongoDb(
                dbFactory: sp =>
                {
                    var mongoDatabaseProvider = sp.GetRequiredService<IMongoDatabaseProvider>();
                    return mongoDatabaseProvider.MongoClient.GetDatabase("apigateway");
                },
                name: "mongo",
                failureStatus: HealthStatus.Unhealthy,
                tags: DatabaseTags,
                timeout: TimeSpan.FromSeconds(5))
            .AddRabbitMQ(
                factory: sp =>
                {
                    var rabbitMQConnection = sp.GetRequiredService<ProducerConnection>();
                    return rabbitMQConnection.Connection;
                },
                name: "rabbitmq",
                failureStatus: HealthStatus.Unhealthy,
                tags: MessagingTags,
                timeout: TimeSpan.FromSeconds(5))
            .AddUrlGroup(
                new Uri("https://httpbin.org/status/200"),
                name: "external-api",
                tags: ApiTags,
                timeout: TimeSpan.FromSeconds(5));

        // Set up the HealthChecks UI with in-memory storage. The HealthChecks UI is a user interface for displaying the results of health checks.
        // By using in-memory storage, the health check results will be stored in memory and will not persist across application restarts.
        services
            .AddHealthChecksUI(setup =>
            {
                setup.SetEvaluationTimeInSeconds(10); // Poll every 10 seconds
                setup.MaximumHistoryEntriesPerEndpoint(50);
                setup.AddHealthCheckEndpoint("Platform Health", "http://localhost:8080/health");
            })
            .AddInMemoryStorage();

        return services;
    }

    /// <summary>
    /// Configures the application to use health check endpoints.
    /// </summary>
    /// <param name="app">The WebApplication to configure.</param>
    /// <returns>The configured WebApplication.</returns>
    public static WebApplication UseHealthCheck(this WebApplication app)
    {
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.MapHealthChecksUI(options => options.UIPath = "/health-ui");

        return app;
    }
}