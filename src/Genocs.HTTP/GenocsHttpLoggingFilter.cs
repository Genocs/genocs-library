using Genocs.HTTP.Options;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Genocs.HTTP;

// Credits goes to https://www.stevejgordon.co.uk/httpclientfactory-asp-net-core-logging
internal sealed class GenocsHttpLoggingFilter : IHttpMessageHandlerBuilderFilter
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly HttpClientSettings _options;

    public GenocsHttpLoggingFilter(ILoggerFactory loggerFactory, HttpClientSettings options)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        _options = options;
    }

    public Action<HttpMessageHandlerBuilder> Configure(Action<HttpMessageHandlerBuilder> next)
    {
        if (next is null)
        {
            throw new ArgumentNullException(nameof(next));
        }

        return builder =>
        {
            next(builder);
            var logger = _loggerFactory.CreateLogger($"System.Net.Http.HttpClient.{builder.Name}.LogicalHandler");
            builder.AdditionalHandlers.Insert(0, new GenocsLoggingScopeHttpMessageHandler(logger, _options));
        };
    }
}