using Genocs.Common.Interfaces;

namespace Genocs.Core.Domain.Entities.Auditing;

public interface IAuditService : ITransientService
{
    Task<List<AuditDto>> GetUserTrailsAsync(DefaultIdType userId);
}