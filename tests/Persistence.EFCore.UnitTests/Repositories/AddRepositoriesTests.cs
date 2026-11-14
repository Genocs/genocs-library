using System.Reflection;
using Genocs.Common.Domain.Entities;
using Genocs.Core.Domain.Repositories;
using Genocs.Persistence.EFCore.Context;
using Genocs.Persistence.EFCore.Extensions;
using Genocs.Persistence.EFCore.Repositories;
using Microsoft.EntityFrameworkCore;
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
        services.Any(d => d.ServiceType == typeof(IReadRepository<>)).ShouldBeTrue();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // IReadRepository<T> is an open generic, so a discovered aggregate resolves
    // without a closed registration.
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AddRepositories_ResolvesReadRepository_ForDiscoveredAggregates()
    {
        var services = new ServiceCollection();
        services.AddRepositories(TestAssembly);
        RegisterApplicationDbContext(services);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        scope.ServiceProvider.GetRequiredService<IReadRepository<ProductAggregate>>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<IReadRepository<OrderAggregate>>().ShouldNotBeNull();
        scope.ServiceProvider.GetRequiredService<IReadRepository<InvoiceAggregate>>().ShouldNotBeNull();
    }

    // ──────────────────────────────────────────────────────────────────────────
    // IRepositoryWithEvents<T> is registered for aggregates that implement
    // IGeneratesDomainEvents, including a normal IAggregateRoot<TKey>.
    // ──────────────────────────────────────────────────────────────────────────

    [Fact]
    public void AddRepositories_RegistersRepositoryWithEvents_ForSelfReferentialAggregate()
    {
        var services = new ServiceCollection();

        services.AddRepositories(TestAssembly);

        services.Any(d => d.ServiceType == typeof(IRepositoryWithEvents<InvoiceAggregate>))
            .ShouldBeTrue();
    }

    [Fact]
    public void AddRepositories_RegistersRepositoryWithEvents_ForTypedKeyAggregate()
    {
        var services = new ServiceCollection();

        services.AddRepositories(TestAssembly);

        services.Any(d => d.ServiceType == typeof(IRepositoryWithEvents<OrderAggregate>))
            .ShouldBeTrue();
    }

    [Fact]
    public void AddRepositories_DoesNotRegisterRepositoryWithEvents_WhenAggregateDoesNotGenerateEvents()
    {
        // ProductAggregate only implements the non-generic IAggregateRoot and has no event collection.
        var services = new ServiceCollection();

        services.AddRepositories(TestAssembly);

        services.Any(d => d.ServiceType == typeof(IRepositoryWithEvents<ProductAggregate>))
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

        services.Any(d => d.ServiceType == typeof(IRepositoryWithEvents<OrderAggregate>))
            .ShouldBeTrue();
        services.Any(d => d.ServiceType == typeof(IRepositoryWithEvents<InvoiceAggregate>))
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

        var eventRepoCount = services.Count(d => d.ServiceType == typeof(IRepositoryWithEvents<OrderAggregate>));
        eventRepoCount.ShouldBe(1, "Passing the same assembly twice must not produce duplicate registrations.");
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

    [Fact]
    public void AddGenocsRepositories_RegistersReadAndEventRepositories_ForDomainAssemblyOutsideGenocsCommon()
    {
        var services = new ServiceCollection();

        services.AddGenocsRepositories<CallerDbContext>(TestAssembly);

        services.Any(d => d.ServiceType == typeof(IReadRepository<>)).ShouldBeTrue();
        services.Any(d => d.ServiceType == typeof(IRepositoryWithEvents<OrderAggregate>))
            .ShouldBeTrue();
        TestAssembly.ShouldNotBe(typeof(IAggregateRoot).Assembly);
    }

    [Fact]
    public void AddGenocsRepositories_RegistersNoClosedAggregateRepositories_WhenScanningGenocsCommon()
    {
        var services = new ServiceCollection();
        var commonAssembly = typeof(IAggregateRoot).Assembly;

        services.AddGenocsRepositories<CallerDbContext>(commonAssembly);

        services.Any(d =>
                d.ServiceType.IsGenericType
                && !d.ServiceType.IsGenericTypeDefinition
                && d.ServiceType.GetGenericArguments()[0].Assembly == commonAssembly)
            .ShouldBeFalse();
    }

    [Fact]
    public void AddGenocsRepositories_ResolvesRepository_WithCallerDbContext()
    {
        var services = new ServiceCollection();
        services.AddSingleton(new DbContextOptionsBuilder<CallerDbContext>().Options);
        services.AddScoped<CallerDbContext>();
        services.AddGenocsRepositories<CallerDbContext>(TestAssembly);

        var repositoryDescriptor = services.Single(d => d.ServiceType == typeof(IRepository<>));
        var constructor = repositoryDescriptor.ImplementationType!.GetConstructors().Single();
        constructor.GetParameters().Single().ParameterType.ShouldBe(typeof(CallerDbContext));

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IRepository<OrderAggregate>>();
        var readRepository = scope.ServiceProvider.GetRequiredService<IReadRepository<OrderAggregate>>();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IRepositoryWithEvents<OrderAggregate>>();

        repository.ShouldBeAssignableTo<GenocsRepository<OrderAggregate, CallerDbContext>>();
        readRepository.ShouldBeAssignableTo<GenocsRepository<OrderAggregate, CallerDbContext>>();
        ((GenocsRepository<OrderAggregate, CallerDbContext>)repository).Context
            .ShouldBeSameAs(scope.ServiceProvider.GetRequiredService<CallerDbContext>());
        eventRepository.ShouldBeOfType<EventAddingRepositoryDecorator<OrderAggregate>>();
        typeof(CallerDbContext).IsAssignableTo(typeof(ApplicationDbContext)).ShouldBeFalse();
    }

    private static void RegisterApplicationDbContext(IServiceCollection services)
    {
        services.AddSingleton<DbContextOptions>(new DbContextOptionsBuilder<ApplicationDbContext>().Options);
        services.AddScoped<ApplicationDbContext>();
    }
}

/// <summary>
/// Public so the dynamically generated repository can name it in its base type.
/// </summary>
public sealed class CallerDbContext : DbContext
{
    public CallerDbContext(DbContextOptions<CallerDbContext> options)
        : base(options)
    {
    }
}
