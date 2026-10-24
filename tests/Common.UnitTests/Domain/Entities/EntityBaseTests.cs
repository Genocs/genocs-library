using Genocs.Common.Domain.Entities;
using Xunit;

namespace Genocs.Common.UnitTests.Domain.Entities;

public class EntityBaseTests
{
    [Fact]
    public void Equals_ShouldReturnTrue_ForSameReference()
    {
        CustomerEntity entity = new(Guid.NewGuid());

        Assert.True(entity.Equals(entity));
    }

    [Fact]
    public void Equals_ShouldReturnTrue_ForSameIdentityAndType()
    {
        Guid id = Guid.NewGuid();
        CustomerEntity left = new(id);
        CustomerEntity right = new(id);

        Assert.True(left.Equals(right));
        Assert.True(left == right);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForDifferentIdentity()
    {
        CustomerEntity left = new(Guid.NewGuid());
        CustomerEntity right = new(Guid.NewGuid());

        Assert.False(left.Equals(right));
        Assert.True(left != right);
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForTwoTransientEntities()
    {
        CustomerEntity left = new(Guid.Empty);
        CustomerEntity right = new(Guid.Empty);

        Assert.True(((IEntity)left).IsNew());
        Assert.True(((IEntity)right).IsNew());
        Assert.False(left.Equals(right));
    }

    [Fact]
    public void Equals_ShouldReturnFalse_ForUnrelatedTypes_WithSameIdentity()
    {
        Guid id = Guid.NewGuid();
        CustomerEntity left = new(id);
        ProductEntity right = new(id);

        Assert.False(left.Equals(right));
    }

    [Fact]
    public void IsTransient_ShouldReturnTrue_ForNonPositiveIntIdentity()
    {
        NumericEntity zero = new(0);
        NumericEntity negative = new(-2);
        NumericEntity positive = new(3);

        Assert.True(zero.IsTransient());
        Assert.True(negative.IsTransient());
        Assert.False(positive.IsTransient());
    }

    private sealed class CustomerEntity : EntityBase<Guid>
    {
        public CustomerEntity(Guid id)
        {
            Id = id;
        }
    }

    private sealed class ProductEntity : EntityBase<Guid>
    {
        public ProductEntity(Guid id)
        {
            Id = id;
        }
    }

    private sealed class NumericEntity : EntityBase<int>
    {
        public NumericEntity(int id)
        {
            Id = id;
        }
    }
}
