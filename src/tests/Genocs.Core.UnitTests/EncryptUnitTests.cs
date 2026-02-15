using Genocs.Core.Extensions;
using Xunit;

namespace Genocs.Core.UnitTests;

public class EncryptUnitTests
{
    [Fact]
    public void MD5EncryptTest()
    {

        string str = "Hello World!".ToMd5();

        Assert.Equal("ED076287532E86365E841E92BFC50D8C", str);
    }
}
