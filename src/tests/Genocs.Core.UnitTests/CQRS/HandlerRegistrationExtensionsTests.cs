using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Events;
using Genocs.Common.CQRS.Queries;
using Genocs.Core.Builders;
using Genocs.Core.CQRS.Commons;
using Genocs.Core.CQRS.Commands;
using Genocs.Core.CQRS.Events;
using Genocs.Core.CQRS.Queries;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Genocs.Core.UnitTests.CQRS;

public class HandlerRegistrationExtensionsTests
{
    [Fact]
    public void AddHandlers_ProjectFilter_RegistersHandlersAsTransient_AndSkipsDuplicates()
    {
        var services = new ServiceCollection();

        services.AddHandlers("Genocs.Core.UnitTests");
        services.AddHandlers("Genocs.Core.UnitTests");

        ServiceDescriptor commandDescriptor = Assert.Single(
            services.Where(d => d.ServiceType == typeof(ICommandHandler<TestCommand>)));
        ServiceDescriptor eventDescriptor = Assert.Single(
            services.Where(d => d.ServiceType == typeof(IEventHandler<TestEvent>)));
        ServiceDescriptor queryDescriptor = Assert.Single(
            services.Where(d => d.ServiceType == typeof(IQueryHandler<TestQuery, int>)));

        Assert.Equal(ServiceLifetime.Transient, commandDescriptor.Lifetime);
        Assert.Equal(ServiceLifetime.Transient, eventDescriptor.Lifetime);
        Assert.Equal(ServiceLifetime.Transient, queryDescriptor.Lifetime);
    }

    [Fact]
    public void AddHandlers_ProjectFilter_DoesNotRegisterWhenProjectDoesNotMatch()
    {
        var services = new ServiceCollection();

        services.AddHandlers("project-name-that-does-not-exist");

        Assert.DoesNotContain(services, d => d.ServiceType == typeof(ICommandHandler<TestCommand>));
        Assert.DoesNotContain(services, d => d.ServiceType == typeof(IEventHandler<TestEvent>));
        Assert.DoesNotContain(services, d => d.ServiceType == typeof(IQueryHandler<TestQuery, int>));
    }

    [Fact]
    public void BuilderRegistrationPath_UsesSameTransientLifetime_AndSkipsDuplicates()
    {
        var services = new ServiceCollection();
        IGenocsBuilder builder = services.AddGenocs(new ConfigurationBuilder().Build());

        builder.AddCommandHandlers();
        builder.AddEventHandlers();
        builder.AddQueryHandlers();

        // Re-register to ensure deterministic skip strategy prevents duplicate descriptors.
        builder.AddCommandHandlers();
        builder.AddEventHandlers();
        builder.AddQueryHandlers();

        ServiceDescriptor commandDescriptor = Assert.Single(
            services.Where(d => d.ServiceType == typeof(ICommandHandler<TestCommand>)));
        ServiceDescriptor eventDescriptor = Assert.Single(
            services.Where(d => d.ServiceType == typeof(IEventHandler<TestEvent>)));
        ServiceDescriptor queryDescriptor = Assert.Single(
            services.Where(d => d.ServiceType == typeof(IQueryHandler<TestQuery, int>)));

        Assert.Equal(ServiceLifetime.Transient, commandDescriptor.Lifetime);
        Assert.Equal(ServiceLifetime.Transient, eventDescriptor.Lifetime);
        Assert.Equal(ServiceLifetime.Transient, queryDescriptor.Lifetime);
    }

    public sealed record TestCommand : ICommand;

    public sealed class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public Task HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    public sealed record TestEvent : IEvent;

    public sealed class TestEventHandler : IEventHandler<TestEvent>
    {
        public Task HandleAsync(TestEvent @event, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    public sealed record TestQuery(int Value) : IQuery<int>;

    public sealed class TestQueryHandler : IQueryHandler<TestQuery, int>
    {
        public Task<int> HandleAsync(TestQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult(query.Value);
    }
}
