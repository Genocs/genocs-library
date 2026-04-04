using Genocs.Common.CQRS.Events;
using Genocs.Common.Types;
using Xunit;

namespace Genocs.Common.UnitTests.CQRS.Events;

public class RejectedEventTests
{
    [Fact]
    public void ErrorProperty_ShouldMapCodeAndReason()
    {
        IRejectedEvent sut = new RejectedEvent("validation failed", "rejection.create_order.failed");

        Assert.Equal("rejection.create_order.failed", sut.Error.Code);
        Assert.Equal("validation failed", sut.Error.Message);
    }

    [Fact]
    public void ForError_ShouldCreateRejectedEventWithSameValues()
    {
        Error error = new("rejection.place_order.failed", "Order cannot be placed.");

        IRejectedEvent sut = RejectedEvent.For(error);

        Assert.Equal(error.Code, sut.Code);
        Assert.Equal(error.Message, sut.Reason);
    }

    [Fact]
    public void ForName_ShouldUseStructuredRejectionCode()
    {
        IRejectedEvent sut = RejectedEvent.For("Create Order");

        Assert.Equal("rejection.create_order.failed", sut.Code);
    }

    [Fact]
    public void RejectionCodeCreate_ShouldNormalizeSegments()
    {
        string code = RejectionCode.Create("Create.Order", "Validation Failed", "Business Domain");

        Assert.Equal("business_domain.create_order.validation_failed", code);
    }

    [Fact]
    public void RejectionCodeTryParse_ShouldReturnParts_WhenValid()
    {
        bool parsed = RejectionCode.TryParse("rejection.create_order.failed", out RejectionCodeParts parts);

        Assert.True(parsed);
        Assert.Equal("rejection", parts.Category);
        Assert.Equal("create_order", parts.Subject);
        Assert.Equal("failed", parts.Reason);
    }

    [Fact]
    public void RejectionCodeTryParse_ShouldFail_WhenInvalid()
    {
        bool parsed = RejectionCode.TryParse("create_order_failed", out _);

        Assert.False(parsed);
    }
}
