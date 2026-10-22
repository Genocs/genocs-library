using Genocs.Common.Domain.Entities;
using Genocs.Common.Domain.Repositories;
using Genocs.Common.Domain.Specifications;
using Xunit;

namespace Genocs.Common.UnitTests.Domain.Repositories;

public class SpecificationContractsTests
{
    [Fact]
    public void SpecificationRepository_ShouldExtendRepositoryOfEntity()
    {
        Assert.Contains(
            typeof(IRepositoryOfEntity<TestEntity, Guid>),
            typeof(ISpecificationRepository<TestEntity, Guid>).GetInterfaces());
    }

    [Fact]
    public void Specification_ShouldExposeFilteringIncludingOrderingAndPagingMembers()
    {
        var properties = typeof(ISpecification<TestEntity>).GetProperties();

        Assert.Contains(properties, property => property.Name == "Criteria");
        Assert.Contains(properties, property => property.Name == "Includes");
        Assert.Contains(properties, property => property.Name == "IncludeStrings");
        Assert.Contains(properties, property => property.Name == "OrderByExpressions");
        Assert.Contains(properties, property => property.Name == "OrderByDescendingExpressions");
        Assert.Contains(properties, property => property.Name == "Skip");
        Assert.Contains(properties, property => property.Name == "Take");
    }

    [Fact]
    public void ProjectionSpecification_ShouldExposeSelector()
    {
        var selector = typeof(IProjectionSpecification<TestEntity, TestProjection>)
            .GetProperty("Selector");

        Assert.NotNull(selector);
    }

    [Fact]
    public void SpecificationRepository_ShouldExposeSpecificationQueryMethods()
    {
        var methods = typeof(ISpecificationRepository<TestEntity, Guid>).GetMethods();

        Assert.Contains(methods, method => method.Name == "ListAsync" && !method.IsGenericMethod);
        Assert.Contains(methods, method => method.Name == "ListAsync" && method.IsGenericMethod);
        Assert.Contains(methods, method => method.Name == "FirstOrDefaultAsync");
        Assert.Contains(methods, method => method.Name == "SingleAsync");
        Assert.Contains(methods, method => method.Name == "CountAsync");
        Assert.Contains(methods, method => method.Name == "LongCountAsync");
    }

    private sealed class TestEntity : IEntity<Guid>
    {
        public Guid Id { get; set; }

        public bool IsTransient() => Id == Guid.Empty;

        public bool IsNew() => IsTransient();
    }

    private sealed record TestProjection(Guid Id);
}
