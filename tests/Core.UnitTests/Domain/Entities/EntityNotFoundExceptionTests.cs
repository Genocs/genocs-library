using Genocs.Core.Domain.Entities;
using Xunit;

namespace Genocs.Core.UnitTests.Domain.Entities;

public class EntityNotFoundExceptionTests
{
    [Fact]
    public void TypeAndIdConstructor_SetsConsistentState()
    {
        var ex = new EntityNotFoundException(typeof(EntityNotFoundExceptionTests), 42);

        Assert.Equal(typeof(EntityNotFoundExceptionTests), ex.EntityType);
        Assert.Equal(42, ex.Id);
        Assert.Contains("EntityNotFoundExceptionTests", ex.Message);
        Assert.Contains("42", ex.Message);
    }

    [Fact]
    public void TypeAndNullIdConstructor_SetsFallbackId()
    {
        var ex = new EntityNotFoundException(typeof(EntityNotFoundExceptionTests), null);

        Assert.Equal(typeof(EntityNotFoundExceptionTests), ex.EntityType);
        Assert.NotNull(ex.Id);
    }

    [Fact]
    public void TypeAndIdWithInnerException_SetsConsistentState()
    {
        var inner = new InvalidOperationException("inner");

        var ex = new EntityNotFoundException(typeof(EntityNotFoundExceptionTests), "abc", inner);

        Assert.Equal(typeof(EntityNotFoundExceptionTests), ex.EntityType);
        Assert.Equal("abc", ex.Id);
        Assert.Same(inner, ex.InnerException);
    }

    [Fact]
    public void MessageConstructor_SetsFallbackState()
    {
        var ex = new EntityNotFoundException("custom message");

        Assert.Equal(typeof(object), ex.EntityType);
        Assert.Equal("<unknown>", ex.Id);
        Assert.Equal("custom message", ex.Message);
    }

    [Fact]
    public void MessageWithInnerConstructor_SetsFallbackState()
    {
        var inner = new Exception("boom");

        var ex = new EntityNotFoundException("custom message", inner);

        Assert.Equal(typeof(object), ex.EntityType);
        Assert.Equal("<unknown>", ex.Id);
        Assert.Equal("custom message", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}
