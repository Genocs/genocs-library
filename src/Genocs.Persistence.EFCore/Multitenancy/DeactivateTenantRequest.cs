using FluentValidation;
using Genocs.Core.Validations;
using Genocs.Persistence.EFCore.MultiTenancy;
using MediatR;

namespace Genocs.Persistence.EFCore.Multitenancy;

public class DeactivateTenantRequest(string tenantId) : IRequest<string>
{
    public string TenantId { get; set; } = tenantId;
}

public class DeactivateTenantRequestValidator : CustomValidator<DeactivateTenantRequest>
{
    public DeactivateTenantRequestValidator()
        => RuleFor(t => t.TenantId)
            .NotEmpty();
}

public class DeactivateTenantRequestHandler(ITenantService tenantService) : IRequestHandler<DeactivateTenantRequest, string>
{
    private readonly ITenantService _tenantService = tenantService;

    public Task<string> Handle(DeactivateTenantRequest request, CancellationToken cancellationToken)
        => _tenantService.DeactivateAsync(request.TenantId);
}