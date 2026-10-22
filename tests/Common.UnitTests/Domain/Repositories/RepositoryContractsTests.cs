using Genocs.Common.Domain.Entities;
using Genocs.Common.Domain.Repositories;
using Xunit;

namespace Genocs.Common.UnitTests.Domain.Repositories;

public class RepositoryContractsTests
{
    [Fact]
    public void RepositoryOfEntity_ShouldNotExposeQueryableMethods()
    {
        var methodNames = typeof(IRepositoryOfEntity<TestEntity, Guid>)
            .GetMethods()
            .Select(method => method.Name)
            .ToHashSet(StringComparer.Ordinal);

        Assert.DoesNotContain("GetAll", methodNames);
        Assert.DoesNotContain("GetAllIncluding", methodNames);
    }

    [Fact]
    public void IQueryableRepository_ShouldExposeQueryableMethods()
    {
        var methods = typeof(IQueryableRepository<TestEntity, Guid>).GetMethods();

        Assert.Contains(methods, method => method.Name == "GetAll" && method.ReturnType == typeof(IQueryable<TestEntity>));
        Assert.Contains(methods, method =>
            method.Name == "GetAllIncluding"
            && method.ReturnType == typeof(IQueryable<TestEntity>)
            && method.GetParameters() is [{ ParameterType.IsArray: true }]);
    }

    [Fact]
    public void IQueryableRepository_ShouldExtendRepositoryOfEntity()
    {
        Assert.Contains(typeof(IRepositoryOfEntity<TestEntity, Guid>), typeof(IQueryableRepository<TestEntity, Guid>).GetInterfaces());
    }

    private sealed class TestEntity : IEntity<Guid>
    {
        public Guid Id { get; set; }

        public bool IsTransient()
        {
            return Id == Guid.Empty;
        }

        public bool IsNew()
        {
            return IsTransient();
        }
    }
}