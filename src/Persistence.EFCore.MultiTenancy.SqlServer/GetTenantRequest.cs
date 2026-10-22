using FluentValidation;
using Genocs.Core.Validations;
using Genocs.Persistence.EFCore.MultiTenancy;
using MediatR;

namespace Genocs.Persistence.EFCore.Multitenancy;

public class GetTenantRequest(string tenantId) : IRequest<TenantDto>
{
    public string TenantId { get; set; } = tenantId;
}

public class GetTenantRequestValidator : CustomValidator<GetTenantRequest>
{
    public GetTenantRequestValidator()
        => RuleFor(t => t.TenantId)
            .NotEmpty();
}

public class GetTenantRequestHandler(ITenantService tenantService) : IRequestHandler<GetTenantRequest, TenantDto>
{
    private readonly ITenantService _tenantService = tenantService;

    public Task<TenantDto> Handle(GetTenantRequest request, CancellationToken cancellationToken)
        => _tenantService.GetByIdAsync(request.TenantId);
}