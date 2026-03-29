using Genocs.Common.Collections;
using Xunit;

namespace Genocs.Common.UnitTests.Collections;

public class TypeListTests
{
    [Fact]
    public void Insert_ShouldThrow_WhenTypeDoesNotMatchBaseType()
    {
        TypeList<Stream> sut = new();

        ArgumentException exception = Assert.Throws<ArgumentException>(() => sut.Insert(0, typeof(string)));

        Assert.Equal("item", exception.ParamName);
    }

    [Fact]
    public void Insert_ShouldAddType_WhenTypeMatchesBaseType()
    {
        TypeList<Stream> sut = new();

        sut.Insert(0, typeof(MemoryStream));

        Assert.Single(sut);
        Assert.Equal(typeof(MemoryStream), sut[0]);
    }
}