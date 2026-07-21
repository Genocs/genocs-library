using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Genocs.Library.Demo.WebApi.Extensions.Checks;

public class MongoDbHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        // Here you would implement the actual logic to check the health of your MongoDB connection.
        // For demonstration purposes, we'll just return a healthy status.
        bool isMongoDbHealthy = true; // Replace with actual health check logic
        if (isMongoDbHealthy)
        {
            return Task.FromResult(HealthCheckResult.Healthy("MongoDB is healthy."));
        }
        else
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("MongoDB is unhealthy."));
        }
    }
}
