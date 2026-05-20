using Genocs.Common.Domain.Entities.Auditing;
using Genocs.Core.Extensions;

namespace Genocs.Core.Domain.Entities.Auditing;

public static class EntityAuditingHelper
{
    private static Func<DateTime> UtcNowProvider { get; } = static () => DateTime.UtcNow;

    public static void SetCreationAuditProperties(
        object entityAsObj,
        int? tenantId,
        DefaultIdType? userId)
    {
        _ = tenantId;

        if (entityAsObj is not IHasCreationTime entityWithCreationTime)
        {
            return;
        }

        if (entityAsObj is not ICreationAudited entity)
        {
            return;
        }

        if (entityWithCreationTime.CreatedAt == default)
        {
            var createdAtProperty = entityAsObj.GetType().GetProperty(nameof(IHasCreationTime.CreatedAt));
            if (createdAtProperty?.CanWrite == true)
            {
                createdAtProperty.SetValue(entityAsObj, UtcNowProvider());
            }
        }

        if (!userId.HasValue)
        {
            return;
        }

        if (entity.CreatorUserId != default)
        {
            return;
        }

        entity.CreatorUserId = userId.Value;
    }

    public static void SetModificationAuditProperties(
        object entityAsObj,
        int? tenantId,
        DefaultIdType? userId)
    {
        _ = tenantId;

        if (entityAsObj is IHasModificationTime)
        {
            entityAsObj.As<IHasModificationTime>().LastUpdate = UtcNowProvider();
        }

        if (entityAsObj is not IModificationAudited)
        {
            return;
        }

        var entity = entityAsObj.As<IModificationAudited>();

        if (userId == null)
        {
            entity.UpdatedBy = null;
            return;
        }

        entity.UpdatedBy = userId;
    }
}
