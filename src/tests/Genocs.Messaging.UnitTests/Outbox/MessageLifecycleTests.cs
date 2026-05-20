using Genocs.Messaging.Outbox.Messages;
using Xunit;

namespace Genocs.Messaging.UnitTests.Outbox;

public class MessageLifecycleTests
{
    [Fact]
    public void InboxMessage_IsTransient_WhenIdIsMissing()
    {
        var message = new InboxMessage();

        Assert.True(message.IsTransient());
    }

    [Fact]
    public void InboxMessage_IsTransient_WhenIdIsWhitespace()
    {
        var message = new InboxMessage { Id = "   " };

        Assert.True(message.IsTransient());
    }

    [Fact]
    public void InboxMessage_IsNotTransient_WhenIdIsSet()
    {
        var message = new InboxMessage { Id = "inbox-1" };

        Assert.False(message.IsTransient());
    }

    [Fact]
    public void OutboxMessage_IsTransient_WhenIdIsMissing()
    {
        var message = new OutboxMessage();

        Assert.True(message.IsTransient());
    }

    [Fact]
    public void OutboxMessage_IsTransient_WhenIdIsWhitespace()
    {
        var message = new OutboxMessage { Id = "\t" };

        Assert.True(message.IsTransient());
    }

    [Fact]
    public void OutboxMessage_IsNotTransient_WhenIdIsSet()
    {
        var message = new OutboxMessage { Id = "outbox-1" };

        Assert.False(message.IsTransient());
    }
}
