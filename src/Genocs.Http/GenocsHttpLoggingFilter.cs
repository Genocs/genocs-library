using Genocs.Http.Configurations;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Genocs.Http;

// Credits goes to https://www.stevejgordon.co.uk/httpclientfactory-asp-net-core-logging
internal sealed class GenocsHttpLoggingFilter(ILoggerFactory loggerFactory, HttpClientOptions options) : IHttpMessageHandlerBuilderFilter
{
    private readonly ILoggerFactory _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    private readonly HttpClientOptions _options = options;

    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        return next is null
            ? throw new ArgumentNullException(nameof(next))
            : (builder =>
            {
                next(builder);

                var logger = _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.LogicalHandler");
                builder.AdditionalHandlers.Insert(0, new GenocsLoggingScopeHttpMessageHandler(logger, _options));
            });
    }
}