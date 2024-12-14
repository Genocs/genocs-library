using Genocs.HTTP;
using Genocs.HTTP.Configurations;
using Genocs.MessageBrokers;
using Microsoft.AspNetCore.Http;

namespace Genocs.Identities.Application;

internal class CorrelationIdFactory : ICorrelationIdFactory
{
    private static readonly AsyncLocal<CorrelationIdHolder> Holder = new AsyncLocal<CorrelationIdHolder>();

    private readonly IMessagePropertiesAccessor _messagePropertiesAccessor;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly string? _header;

    public CorrelationIdFactory(
                                IMessagePropertiesAccessor messagePropertiesAccessor,
                                IHttpContextAccessor httpContextAccessor,
                                HttpClientOptions httpClientOptions)
    {
        ArgumentNullException.ThrowIfNull(httpClientOptions);

        _messagePropertiesAccessor = messagePropertiesAccessor ?? throw new ArgumentNullException(nameof(messagePropertiesAccessor));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _header = httpClientOptions.CorrelationIdHeader;
    }

    private static string? CorrelationId
    {
        get => Holder.Value?.Id;
        set
        {
            var holder = Holder.Value;
            if (holder is { })
            {
                holder.Id = null;
            }

            if (value is { })
            {
                Holder.Value = new CorrelationIdHolder { Id = value };
            }
        }
    }

    private class CorrelationIdHolder
    {
        public string? Id;
    }

    public string Create()
    {
        if (!string.IsNullOrWhiteSpace(CorrelationId))
        {
            return CorrelationId;
        }

        string? correlationId = _messagePropertiesAccessor.MessageProperties?.CorrelationId;
        if (!string.IsNullOrWhiteSpace(correlationId))
        {
            CorrelationId = correlationId;
            return CorrelationId;
        }

        if (string.IsNullOrWhiteSpace(_header) || _httpContextAccessor.HttpContext is null)
        {
            CorrelationId = CreateId();
            return CorrelationId;
        }

        if (!_httpContextAccessor.HttpContext.Request.Headers.TryGetValue(_header, out var id))
        {
            CorrelationId = CreateId();
            return CorrelationId;
        }

        correlationId = id.ToString();
        CorrelationId = string.IsNullOrWhiteSpace(correlationId) ? CreateId() : correlationId;

        return CorrelationId;
    }

    private static string CreateId() => Guid.NewGuid().ToString("N");
}