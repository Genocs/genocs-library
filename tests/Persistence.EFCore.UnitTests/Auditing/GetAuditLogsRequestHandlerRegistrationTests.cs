using AuditDto = Genocs.Core.Domain.Entities.Auditing.AuditDto;
using Genocs.Persistence.EFCore.Auditing;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Genocs.Persistence.EFCore.UnitTests.Auditing;

public class GetAuditLogsRequestHandlerRegistrationTests
{
    [Fact]
    public void AddMediatR_OnPersistenceAssembly_DoesNotRequireUnregisteredServices()
    {
        var services = new ServiceCollection();
        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssembly(typeof(GetAuditLogsRequest).Assembly));

        using var provider = services.BuildServiceProvider();

        var ex = Record.Exception(() =>
            provider.GetServices<IRequestHandler<GetAuditLogsRequest, List<AuditDto>>>().ToList());

        ex.ShouldBeNull();
        services.ShouldNotContain(descriptor =>
            descriptor.ServiceType == typeof(IRequestHandler<GetAuditLogsRequest, List<AuditDto>>));
    }
}
