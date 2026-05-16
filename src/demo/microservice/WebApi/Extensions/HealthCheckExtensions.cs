using Genocs.Library.Demo.WebApi.Extensions.Checks;

namespace Genocs.Library.Demo.WebApi.Extensions;

/// <summary>
/// Extension method for health check related services and middlewares.
/// </summary>
public static class HealthCheckExtensions
{
    /// <summary>
    /// Adds health check services to the dependency injection container. This method registers various health checks for MongoDB, RabbitMQ, and API clients.
    /// </summary>
    /// <param name="services">The IServiceCollection to add the health checks to.</param>
    /// <returns>The IServiceCollection with the health checks added.</returns>
    public static IServiceCollection AddHealthCheck(this IServiceCollection services)
    {
        var healthChecks = services.AddHealthChecks();
        healthChecks.AddCheck<MongoDbHealthCheck>("MongoDB");
        healthChecks.AddCheck<RabbitMqHealthCheck>("RabbitMQ");
        healthChecks.AddCheck<ApiClientsHealthCheck>("API Clients");
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