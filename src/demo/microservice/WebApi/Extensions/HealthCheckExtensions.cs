using Genocs.Library.Demo.WebApi.Extensions.Checks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

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

        // Example of a simple custom health check that always returns healthy.
        // You can replace this with actual logic to check the health of your application.
        healthChecks.AddCheck(
                "HealthCheckName",
                () => HealthCheckResult.Healthy("OK"));

        // Add specific health checks for MongoDB, RabbitMQ, and API clients with appropriate tags for filtering.
        healthChecks.AddCheck<MongoDbHealthCheck>("MongoDB", tags: DatabaseTags);
        healthChecks.AddCheck<RabbitMqHealthCheck>("RabbitMQ", tags: MessagingTags);
        healthChecks.AddCheck<ApiClientsHealthCheck>("API Clients", tags: ApiTags);

        // Configure the health check publisher options to specify a delay before publishing health check results and to filter which checks are published based on tags.
        // The check publisher is responsible for publishing the health check results to a monitoring system or other external service.
        // In this case, we are configuring it to only publish checks that are tagged with "ready" after a delay of 2 seconds.
        services.Configure<HealthCheckPublisherOptions>(options =>
        {
            options.Delay = TimeSpan.FromSeconds(2);
            options.Predicate = check => check.Tags.Contains("ready");
        });

        return services;
    }

    /// <summary>
    /// Configures the application to use health check endpoints.
    /// </summary>
    /// <param name="app">The WebApplication to configure.</param>
    /// <returns>The configured WebApplication.</returns>
    public static WebApplication UseHealthCheck(this WebApplication app)
    {
        app.MapHealthChecks("/health");
        return app;
    }
}