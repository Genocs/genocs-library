using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Genocs.Logging;

public class CorrelationContextLoggingMiddleware : IMiddleware
{
    private readonly ILogger<CorrelationContextLoggingMiddleware> _logger;

    public CorrelationContextLoggingMiddleware(ILogger<CorrelationContextLoggingMiddleware> logger)
       => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var headers = Activity.Current?.Baggage
            .ToDictionary(x => x.Key, x => x.Value);

        if (headers is null)
        {
            return next(context);
        }

        using (_logger.BeginScope(headers))
        {
            return next(context);
        }
    }
}