using MediatR;

namespace Genocs.Persistence.EFCore.MultiTenancy;

public class CreateTenantRequest : IRequest<string>
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string ConnectionString { get; set; }
    public string AdminEmail { get; set; } = default!;
    public string Issuer { get; set; }
}

public class CreateTenantRequestHandler(ITenantService tenantService) : IRequestHandler<CreateTenantRequest, string>
{
    private readonly ITenantService _tenantService = tenantService;

    public Task<string> Handle(CreateTenantRequest request, CancellationToken cancellationToken)
        => _tenantService.CreateAsync(request, cancellationToken);
}