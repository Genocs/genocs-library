using Genocs.Persistence.EFCore.MultiTenancy;
using MediatR;

namespace Genocs.Persistence.EFCore.Multitenancy;

public class GetAllTenantsRequest : IRequest<List<TenantDto>>;

public class GetAllTenantsRequestHandler(ITenantService tenantService) : IRequestHandler<GetAllTenantsRequest, List<TenantDto>>
{
    private readonly ITenantService _tenantService = tenantService;

    public Task<List<TenantDto>> Handle(GetAllTenantsRequest request, CancellationToken cancellationToken)
        => _tenantService.GetAllAsync();
}