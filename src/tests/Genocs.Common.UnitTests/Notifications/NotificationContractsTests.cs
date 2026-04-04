using Genocs.Common.Notifications;
using Xunit;

namespace Genocs.Common.UnitTests.Notifications;

public class NotificationContractsTests
{
    [Fact]
    public void BasicNotification_Message_ShouldThrow_WhenWhitespace()
    {
        BasicNotification sut = new() { Message = "valid" };

        Assert.Throws<ArgumentException>(() => sut.Message = "   ");
    }

    [Fact]
    public void BasicNotification_Message_ShouldAccept_ValidText()
    {
        BasicNotification sut = new() { Message = "Operation completed." };

        Assert.Equal("Operation completed.", sut.Message);
    }

    [Fact]
    public void JobNotification_Message_ShouldThrow_WhenEmpty()
    {
        JobNotification sut = new() { Message = "started", Progress = 0m };

        Assert.Throws<ArgumentException>(() => sut.Message = string.Empty);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    public void JobNotification_Progress_ShouldThrow_WhenOutOfRange(decimal value)
    {
        JobNotification sut = new() { Message = "running", Progress = 50m };

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.Progress = value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void JobNotification_Progress_ShouldAccept_RangeValues(decimal value)
    {
        JobNotification sut = new() { Message = "running", Progress = value };

        Assert.Equal(value, sut.Progress);
    }
}
