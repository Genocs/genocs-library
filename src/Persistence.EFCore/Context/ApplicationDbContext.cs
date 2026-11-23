using System.Data;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Persistence.EFCore.Context;

public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
{
    // Used by Dapper
    public IDbConnection Connection => Database.GetDbConnection();

    /// <summary>
    /// Registers Genocs conventions, including the optimistic concurrency token for <c>IVersioned</c> aggregates.
    /// </summary>
    /// <param name="configurationBuilder">Convention builder.</param>
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddGenocsConventions();
    }

    /// <inheritdoc />
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
        => OptimisticConcurrency.Save(this, () => base.SaveChanges(acceptAllChangesOnSuccess));

    /// <inheritdoc />
    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
        => OptimisticConcurrency.SaveAsync(
            this,
            token => base.SaveChangesAsync(acceptAllChangesOnSuccess, token),
            cancellationToken);
}
