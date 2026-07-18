using MediatR;
using FluentValidation;
using Genocs.Core.Validations;
using Genocs.Persistence.EFCore.MultiTenancy;

namespace Genocs.Persistence.EFCore.Multitenancy;

public class ActivateTenantRequest(string tenantId) : IRequest<string>
{
    public string TenantId { get; set; } = tenantId;
}

public class ActivateTenantRequestValidator : CustomValidator<ActivateTenantRequest>
{
    public ActivateTenantRequestValidator()
        => RuleFor(t => t.TenantId)
            .NotEmpty();
}

public class ActivateTenantRequestHandler(ITenantService tenantService) : IRequestHandler<ActivateTenantRequest, string>
{
    private readonly ITenantService _tenantService = tenantService;

    public Task<string> Handle(ActivateTenantRequest request, CancellationToken cancellationToken)
        => _tenantService.ActivateAsync(request.TenantId);
}