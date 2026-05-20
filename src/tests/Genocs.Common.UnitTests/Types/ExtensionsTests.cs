using Genocs.Common.Types;
using Xunit;

namespace Genocs.Common.UnitTests.Types;

public class ExtensionsTests
{
    [Fact]
    public void SetDefaultInstanceProperties_ShouldPopulatePublicReferenceAndStringProperties()
    {
        DefaultInstanceContainer sut = new();

        sut.SetDefaultInstanceProperties();

        Assert.Equal(string.Empty, sut.Name);
        Assert.NotNull(sut.Child);
        Assert.Equal(string.Empty, sut.Child!.Name);
    }

    [Fact]
    public void GetDefaultInstance_ShouldLimitObjectGraphTraversalDepth()
    {
        object? instance = typeof(Depth01).GetDefaultInstance();

        Assert.NotNull(instance);
        Assert.InRange(CountDepth(instance), 1, 11);
    }

    private static int CountDepth(object instance)
    {
        int depth = 0;
        object? current = instance;

        while (current is not null)
        {
            depth++;
            current = current.GetType().GetProperty("Next")?.GetValue(current);
        }

        return depth;
    }

    public sealed class DefaultInstanceContainer
    {
        public string Name { get; set; } = null!;

        public DefaultInstanceChild? Child { get; set; }
    }

    public sealed class DefaultInstanceChild
    {
        public string Name { get; set; } = null!;
    }

    public sealed class Depth01
    {
        public Depth02? Next { get; set; }
    }

    public sealed class Depth02
    {
        public Depth03? Next { get; set; }
    }

    public sealed class Depth03
    {
        public Depth04? Next { get; set; }
    }

    public sealed class Depth04
    {
        public Depth05? Next { get; set; }
    }

    public sealed class Depth05
    {
        public Depth06? Next { get; set; }
    }

    public sealed class Depth06
    {
        public Depth07? Next { get; set; }
    }

    public sealed class Depth07
    {
        public Depth08? Next { get; set; }
    }

    public sealed class Depth08
    {
        public Depth09? Next { get; set; }
    }

    public sealed class Depth09
    {
        public Depth10? Next { get; set; }
    }

    public sealed class Depth10
    {
        public Depth11? Next { get; set; }
    }

    public sealed class Depth11
    {
        public Depth12? Next { get; set; }
    }

    public sealed class Depth12;
}