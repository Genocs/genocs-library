using System.Reflection;
using Genocs.Common.Domain.Entities;
using Genocs.Core.Domain.Repositories;
using Genocs.Persistence.EFCore.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Genocs.Persistence.EFCore.UnitTests.Repositories;

/// <summary>
/// Tests for the AddRepositories extension (EFCORE-003).
///
/// Validates that repository interfaces are registered for aggregate root types
/// found in explicitly-supplied assemblies, and that non-aggregate types are
/// not picked up.
/// </summary>
public class AddRepositoriesTests
{
    private static Assembly TestAssembly => typeof(AddRepositoriesTests).Assembly;

    // ──────────────────────────────────────────────────────────────────────────
    // Open generic IRepository<> registration
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AddRepositories_RegistersOpenGenericRepository()
    {
        var services = new ServiceCollection();

        services.AddRepositories(TestAssembly);

        services.Any(d => d.ServiceType == typeof(IRepository<>)).ShouldBeTrue();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // IReadRepository<T> registration for discovered types
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AddRepositories_RegistersReadRepository_ForProductAggregate()
    {
        var services = new ServiceCollection();

        services.AddRepositories(TestAssembly);

        services.Any(d => d.ServiceType == typeof(IReadRepository<ProductAggregate>))
            .ShouldBeTrue();
    }

    [Fact]
    public void AddRepositories_RegistersReadRepository_ForOrderAggregate()
    {
        var services = new ServiceCollection();

        services.AddRepositories(TestAssembly);

        services.Any(d => d.ServiceType == typeof(IReadRepository<OrderAggregate>))
            .ShouldBeTrue();
    }

    [Fact]
    public void AddRepositories_RegistersReadRepository_ForInvoiceAggregate()
    {
        var services = new ServiceCollection();

        services.AddRepositories(TestAssembly);

        services.Any(d => d.ServiceType == typeof(IReadRepository<InvoiceAggregate>))
            .ShouldBeTrue();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // IRepositoryWithEvents<T> registration — only for types that satisfy
    // EventAddingRepositoryDecorator<T>'s self-referential IAggregateRoot<T>
    // constraint (see EFCORE-017 for the full constraint relaxation).
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AddRepositories_RegistersRepositoryWithEvents_ForSelfReferentialAggregate()
    {
        // InvoiceAggregate implements IAggregateRoot<InvoiceAggregate> (self-referential),
        // so EventAddingRepositoryDecorator<InvoiceAggregate> is valid.
        var services = new ServiceCollection();

        services.AddRepositories(TestAssembly);

        services.Any(d => d.ServiceType == typeof(IRepositoryWithEvents<InvoiceAggregate>))
            .ShouldBeTrue();
    }

    [Fact]
    public void AddRepositories_DoesNotRegisterRepositoryWithEvents_WhenConstraintNotSatisfied()
    {
        // ProductAggregate only implements the non-generic IAggregateRoot.
        // EventAddingRepositoryDecorator<ProductAggregate> would violate its generic
        // constraint, so IRepositoryWithEvents<ProductAggregate> must not be registered.
        var services = new ServiceCollection();

        services.AddRepositories(TestAssembly);

        services.Any(d => d.ServiceType == typeof(IRepositoryWithEvents<ProductAggregate>))
            .ShouldBeFalse();
    }

    [Fact]
    public void AddRepositories_DoesNotRegisterRepositoryWithEvents_ForTypedKeyAggregate()
    {
        // OrderAggregate implements IAggregateRoot<Guid>, not IAggregateRoot<OrderAggregate>,
        // so the self-referential constraint is not satisfied.
        var services = new ServiceCollection();

        services.AddRepositories(TestAssembly);

        services.Any(d => d.ServiceType == typeof(IRepositoryWithEvents<OrderAggregate>))
            .ShouldBeFalse();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Non-aggregate types must not be registered
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AddRepositories_DoesNotRegisterReadRepository_ForNonAggregateType()
    {
        var services = new ServiceCollection();

        services.AddRepositories(TestAssembly);

        // IReadRepository<T> has a where T : class, IAggregateRoot constraint, so MakeGenericType
        // with NotAnAggregate would throw. Instead, verify by inspecting generic arguments directly.
        services.Any(d =>
                d.ServiceType.IsGenericType
                && d.ServiceType.GetGenericArguments().Any(a => a == typeof(NotAnAggregate)))
            .ShouldBeFalse();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Explicit assembly scoping — interface-only assemblies must not contribute
    // invalid IReadRepository<T> registrations
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AddRepositories_RegistersNoConcreteAggregates_WhenScanningGenocsCommonOnly()
    {
        // Genocs.Common contains only IAggregateRoot interface definitions, not concrete classes.
        // Even though AddRepositories also scans the entry assembly fallback, nothing from
        // Genocs.Common itself should become a closed IReadRepository<T> registration.
        var services = new ServiceCollection();
        var commonAssembly = typeof(IAggregateRoot).Assembly;

        services.AddRepositories(commonAssembly);

        // IRepository<> (open generic) is always registered regardless.
        // Assert that no IReadRepository<T> is registered for any type from Genocs.Common.
        services.Any(d =>
                d.ServiceType.IsGenericType
                && d.ServiceType.GetGenericTypeDefinition() == typeof(IReadRepository<>)
                && d.ServiceType.GetGenericArguments()[0].Assembly == commonAssembly)
            .ShouldBeFalse(
                "Genocs.Common is interface-only for aggregate roots; it must not produce closed IReadRepository<T> registrations.");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Multi-assembly scanning
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AddRepositories_ScansAllSuppliedAssemblies()
    {
        // Supplying both the test assembly and Genocs.Common must register aggregates
        // from the test assembly while ignoring the interface-only Genocs.Common.
        var services = new ServiceCollection();
        var commonAssembly = typeof(IAggregateRoot).Assembly;

        services.AddRepositories(TestAssembly, commonAssembly);

        // Types from the test assembly are present.
        services.Any(d => d.ServiceType == typeof(IReadRepository<ProductAggregate>))
            .ShouldBeTrue();
        services.Any(d => d.ServiceType == typeof(IReadRepository<OrderAggregate>))
            .ShouldBeTrue();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Idempotency guard — duplicate assemblies must not cause double registrations
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AddRepositories_DoesNotRegisterDuplicates_WhenSameAssemblyPassedTwice()
    {
        var services = new ServiceCollection();

        services.AddRepositories(TestAssembly, TestAssembly);

        var readRepoCount = services.Count(d => d.ServiceType == typeof(IReadRepository<ProductAggregate>));
        readRepoCount.ShouldBe(1, "Passing the same assembly twice must not produce duplicate registrations.");
    }

    // ──────────────────────────────────────────────────────────────────────────
    // Empty assembly array — no crash, only open generic IRepository<> registered
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AddRepositories_DoesNotCrash_WhenNoAssembliesProvided()
    {
        // Providing zero assemblies explicitly (not relying on the entry-assembly fallback)
        // must not throw; IRepository<> is still registered as the open generic.
        var services = new ServiceCollection();

        var ex = Record.Exception(() => services.AddRepositories(Array.Empty<Assembly>()));

        ex.ShouldBeNull();
        services.Any(d => d.ServiceType == typeof(IRepository<>)).ShouldBeTrue();
    }
}
