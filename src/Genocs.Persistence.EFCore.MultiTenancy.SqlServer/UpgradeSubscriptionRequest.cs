using FluentValidation;
using Genocs.Core.Validations;
using Genocs.Persistence.EFCore.MultiTenancy;
using MediatR;

namespace Genocs.Persistence.EFCore.Multitenancy;

public class UpgradeSubscriptionRequest : IRequest<string>
{
    public string TenantId { get; set; } = default!;
    public DateTime ExtendedExpiryDate { get; set; }
}

public class UpgradeSubscriptionRequestValidator : CustomValidator<UpgradeSubscriptionRequest>
{
    public UpgradeSubscriptionRequestValidator()
        => RuleFor(t => t.TenantId)
            .NotEmpty();
}

public class UpgradeSubscriptionRequestHandler(ITenantService tenantService) : IRequestHandler<UpgradeSubscriptionRequest, string>
{
    private readonly ITenantService _tenantService = tenantService;

    public Task<string> Handle(UpgradeSubscriptionRequest request, CancellationToken cancellationToken)
        => _tenantService.UpdateSubscriptionAsync(request.TenantId, request.ExtendedExpiryDate);
}