using MediatR;
using AuditDto = Genocs.Core.Domain.Entities.Auditing.AuditDto;

namespace Genocs.Persistence.EFCore.Auditing;

/// <summary>
/// Requests the current user's audit trail.
/// The package does not ship a handler: <c>IAuditService</c> and <c>ICurrentUser</c>
/// are host services, and a concrete <c>IRequestHandler</c> in this assembly would be
/// activated by any host that scans loaded assemblies.
/// </summary>
public class GetAuditLogsRequest : IRequest<List<AuditDto>>;
