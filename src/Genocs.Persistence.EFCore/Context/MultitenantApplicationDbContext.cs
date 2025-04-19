/*
using Finbuckle.MultiTenant.Abstractions;
using Genocs.Common.Interfaces;
using Genocs.Microservice.Template.Application.Common.Events;
using Genocs.Microservice.Template.Application.Common.Interfaces;
using Genocs.Microservice.Template.Domain.Catalog;
using Genocs.Microservice.Template.Infrastructure.Multitenancy;
using Genocs.Microservice.Template.Infrastructure.Persistence.Configuration;
using Genocs.Persistence.EFCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Genocs.Microservice.Template.Infrastructure.Persistence.Context;

public class MultitenantApplicationDbContext : BaseDbContext
{
    public MultitenantApplicationDbContext(IMultiTenantContextAccessor<GNXTenantInfo> multiTenantContextAccessor, DbContextOptions options, ICurrentUser currentUser, ISerializerService serializer, IOptions<DatabaseSettings> dbSettings, IEventPublisher events)
        : base(multiTenantContextAccessor, options, currentUser, serializer, dbSettings, events)
    {
    }


    //public DbSet<Product> Products => Set<Product>();
    //public DbSet<Brand> Brands => Set<Brand>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //modelBuilder.HasDefaultSchema(SchemaNames.Catalog);
    }
}

*/