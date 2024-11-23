using Serilog.Context;

namespace Genocs.APIGateway.Framework;

internal class LogContextMiddleware : IMiddleware
{
    private readonly CorrelationIdFactory _correlationIdFactory;

    public LogContextMiddleware(CorrelationIdFactory correlationIdFactory)
    {
        _correlationIdFactory = correlationIdFactory ?? throw new ArgumentNullException(nameof(correlationIdFactory));
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        string correlationId = _correlationIdFactory.Create();
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}