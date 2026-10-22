using Genocs.Common.Domain.Entities;
using Xunit;

namespace Genocs.Common.UnitTests.Domain.Entities;

public class IEntityLifecycleTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsNew_ShouldMirrorIsTransient_ForIEntityImplementations(bool isTransient)
    {
        IEntity entity = new TestEntity(isTransient);

        Assert.Equal(entity.IsTransient(), entity.IsNew());
    }

    private sealed class TestEntity : IEntity
    {
        private readonly bool isTransient;

        public TestEntity(bool isTransient)
        {
            this.isTransient = isTransient;
        }

        public bool IsTransient() => this.isTransient;
    }
}
