// using Genocs.Timing;
// using Genocs.Core.Configuration.Startup;
// using Genocs.Core.MultiTenancy;
using Genocs.Core.Extensions;

namespace Genocs.Core.Domain.Entities.Auditing;

public static class EntityAuditingHelper
{
    public static void SetCreationAuditProperties(

        // IMultiTenancyConfig multiTenancyConfig,
        object entityAsObj,
        int? tenantId,
        DefaultIdType? userId)
    {
        var entityWithCreationTime = entityAsObj as IHasCreationTime;
        if (entityWithCreationTime == null)
        {
            // Object does not implement IHasCreationTime
            return;
        }

        if (entityWithCreationTime.CreatedAt == default)
        {
            // entityWithCreationTime.CreationTime = Clock.Now;
            // entityWithCreationTime.CreatedAt = DateTime.Now;
        }

        if (!(entityAsObj is ICreationAudited))
        {
            // Object does not implement ICreationAudited
            return;
        }

        if (!userId.HasValue)
        {
            // Unknown user
            return;
        }

        var entity = entityAsObj as ICreationAudited;
        if (entity.CreatorUserId != null)
        {
            // CreatorUserId is already set
            return;
        }

        //if (multiTenancyConfig?.IsEnabled == true)
        //{
        //    if (MultiTenancyHelper.IsMultiTenantEntity(entity) &&
        //        !MultiTenancyHelper.IsTenantEntity(entity, tenantId))
        //    {
        //        //A tenant entitiy is created by host or a different tenant
        //        return;
        //    }

        //    if (tenantId.HasValue && MultiTenancyHelper.IsHostEntity(entity))
        //    {
        //        //Tenant user created a host entity
        //        return;
        //    }
        //}

        // Finally, set CreatorUserId!
        entity.CreatorUserId = userId.Value;
    }

    public static void SetModificationAuditProperties(

        // IMultiTenancyConfig multiTenancyConfig,
        object entityAsObj,
        int? tenantId,
        DefaultIdType? userId)
    {
        if (entityAsObj is IHasModificationTime)
        {
            // entityAsObj.As<IHasModificationTime>().LastModificationTime = Clock.Now;
            entityAsObj.As<IHasModificationTime>().LastUpdate = DateTime.Now;
        }

        if (!(entityAsObj is IModificationAudited))
        {
            // Entity does not implement IModificationAudited
            return;
        }

        var entity = entityAsObj.As<IModificationAudited>();

        if (userId == null)
        {
            // Unknown user
            entity.UpdatedBy = null;
            return;
        }

        /*
        if (multiTenancyConfig?.IsEnabled == true)
        {
            if (MultiTenancyHelper.IsMultiTenantEntity(entity) &&
                !MultiTenancyHelper.IsTenantEntity(entity, tenantId))
            {
                //A tenant entity is modified by host or a different tenant
                entity.LastModifierUserId = null;
                return;
            }

            if (tenantId.HasValue && MultiTenancyHelper.IsHostEntity(entity))
            {
                //Tenant user modified a host entity
                entity.LastModifierUserId = null;
                return;
            }
        }
        */

        // Finally, set LastModifierUserId!
        entity.UpdatedBy = userId;
    }
}
