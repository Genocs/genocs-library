using Genocs.Messaging;
using Xunit;

namespace Genocs.Messaging.UnitTests.Core;

public sealed class MessagingAccessorsTests
{
    [Fact]
    public void CorrelationContextAccessor_SetValue_ReturnsSameInstance()
    {
        var accessor = new CorrelationContextAccessor();
        var context = new { CorrelationId = Guid.NewGuid().ToString("N") };

        accessor.CorrelationContext = context;

        Assert.Same(context, accessor.CorrelationContext);
        accessor.CorrelationContext = null;
    }

    [Fact]
    public void CorrelationContextAccessor_SetNull_ClearsCurrentValue()
    {
        var accessor = new CorrelationContextAccessor();
        accessor.CorrelationContext = new { CorrelationId = "ctx-1" };

        accessor.CorrelationContext = null;

        Assert.Null(accessor.CorrelationContext);
    }

    [Fact]
    public async Task CorrelationContextAccessor_FlowsAcrossAwaitBoundary()
    {
        var accessor = new CorrelationContextAccessor();
        var context = new { CorrelationId = "ctx-await" };
        accessor.CorrelationContext = context;

        await Task.Yield();

        Assert.Same(context, accessor.CorrelationContext);
        accessor.CorrelationContext = null;
    }

    [Fact]
    public void MessagePropertiesAccessor_SetValue_ReturnsSameInstance()
    {
        var accessor = new MessagePropertiesAccessor();
        IMessageProperties messageProperties = new MessageProperties
        {
            MessageId = Guid.NewGuid().ToString("N"),
            CorrelationId = Guid.NewGuid().ToString("N"),
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            Headers = new Dictionary<string, object?> { ["tenant"] = "alpha" }
        };

        accessor.MessageProperties = messageProperties;

        Assert.Same(messageProperties, accessor.MessageProperties);
        accessor.MessageProperties = null;
    }

    [Fact]
    public void MessagePropertiesAccessor_SetNull_ClearsCurrentValue()
    {
        var accessor = new MessagePropertiesAccessor();
        accessor.MessageProperties = new MessageProperties { MessageId = "msg-1" };

        accessor.MessageProperties = null;

        Assert.Null(accessor.MessageProperties);
    }

    [Fact]
    public async Task MessagePropertiesAccessor_FlowsAcrossAwaitBoundary()
    {
        var accessor = new MessagePropertiesAccessor();
        IMessageProperties messageProperties = new MessageProperties
        {
            MessageId = "msg-await",
            CorrelationId = "corr-await",
            Timestamp = 123,
            Headers = new Dictionary<string, object?>()
        };
        accessor.MessageProperties = messageProperties;

        await Task.Yield();

        Assert.Same(messageProperties, accessor.MessageProperties);
        accessor.MessageProperties = null;
    }
}