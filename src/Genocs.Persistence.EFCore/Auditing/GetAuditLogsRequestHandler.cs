using Genocs.Common.Interfaces;
using Genocs.Core.Domain.Entities.Auditing;
using MediatR;

namespace Genocs.Persistence.EFCore.Auditing;

public class GetAuditLogsRequest : IRequest<List<AuditDto>>;

public class GetAuditLogsRequestHandler : IRequestHandler<GetAuditLogsRequest, List<AuditDto>>
{
    private readonly ICurrentUser _currentUser;
    private readonly IAuditService _auditService;

    public GetAuditLogsRequestHandler(ICurrentUser currentUser, IAuditService auditService) =>
        (_currentUser, _auditService) = (currentUser, auditService);

    public Task<List<AuditDto>> Handle(GetAuditLogsRequest request, CancellationToken cancellationToken) =>
        _auditService.GetUserTrailsAsync(_currentUser.GetUserId());
}