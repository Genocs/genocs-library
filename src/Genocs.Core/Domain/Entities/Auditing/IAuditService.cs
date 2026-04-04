using Genocs.Common.Dependency;

namespace Genocs.Core.Domain.Entities.Auditing;

public interface IAuditService : ITransientDependency
{
    Task<List<AuditDto>> GetUserTrailsAsync(DefaultIdType userId, CancellationToken cancellationToken = default);
}