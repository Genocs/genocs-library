using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Genocs.Library.Demo.WebApi.Extensions.Checks;

public class RabbitMqHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Here you would implement the actual logic to check the health of your RabbitMQ connection.
        // For demonstration purposes, we'll just return a healthy status.
        bool isRabbitMqHealthy = true; // Replace with actual health check logic
        if (isRabbitMqHealthy)
        {
            return Task.FromResult(HealthCheckResult.Healthy("RabbitMQ is healthy."));
        }
        else
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ is unhealthy."));
        }
    }
}
