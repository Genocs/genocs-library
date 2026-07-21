using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Genocs.Library.Demo.WebApi.Extensions.Checks;

public class ApiClientsHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Here you would implement the actual logic to check the health of your API clients.
        // For demonstration purposes, we'll just return a healthy status.
        bool areApiClientsHealthy = true; // Replace with actual health check logic
        if (areApiClientsHealthy)
        {
            return Task.FromResult(HealthCheckResult.Healthy("API clients are healthy."));
        }
        else
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("API clients are unhealthy."));
        }
    }
}
