using System.Net;
using Genocs.Common.CQRS.Events;
using Genocs.Common.Domain.Entities;
using Genocs.Persistence.EFCore.Context;
using Genocs.Persistence.EFCore.Exceptions;
using Genocs.Persistence.EFCore.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Genocs.Persistence.EFCore.UnitTests.Repositories;

public class OptimisticConcurrencyTests
{
    [Fact]
    public async Task UpdateAsync_IncrementsVersion_WhenWriteIsAccepted()
    {
        await using var database = await SqliteDatabase<CallerVersionedDbContext>.CreateAsync();
        var id = Guid.NewGuid();

        await using (var context = database.CreateContext())
        {
            var repository = new GenocsRepository<VersionedOrder, CallerVersionedDbContext>(context);
            var order = new VersionedOrder { Id = id, Name = "draft" };

            var added = await repository.AddAsync(order);

            added.Version.ShouldBe(0);
        }

        await using (var context = database.CreateContext())
        {
            var repository = new GenocsRepository<VersionedOrder, CallerVersionedDbContext>(context);
            var order = await context.Set<VersionedOrder>().SingleAsync(candidate => candidate.Id == id);
            order.Name = "accepted";

            await repository.UpdateAsync(order);

            order.Version.ShouldBe(1);
        }

        await using var verify = database.CreateContext();
        var stored = await verify.Set<VersionedOrder>().SingleAsync(candidate => candidate.Id == id);
        stored.Name.ShouldBe("accepted");
        stored.Version.ShouldBe(1);
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenStoredVersionIsNewer()
    {
        await using var database = await SqliteDatabase<CallerVersionedDbContext>.CreateAsync();
        var id = Guid.NewGuid();
        await InsertAsync(database, id, "original");

        VersionedOrder stale;
        await using (var context = database.CreateContext())
        {
            stale = await context.Set<VersionedOrder>().SingleAsync(candidate => candidate.Id == id);
        }

        await using (var context = database.CreateContext())
        {
            var repository = new GenocsRepository<VersionedOrder, CallerVersionedDbContext>(context);
            var current = await context.Set<VersionedOrder>().SingleAsync(candidate => candidate.Id == id);
            current.Name = "winner";
            await repository.UpdateAsync(current);
        }

        await using (var context = database.CreateContext())
        {
            var repository = new GenocsRepository<VersionedOrder, CallerVersionedDbContext>(context);
            stale.Name = "loser";

            var exception = await Should.ThrowAsync<OptimisticConcurrencyException>(() => repository.UpdateAsync(stale));

            exception.StatusCode.ShouldBe(HttpStatusCode.Conflict);
            exception.EntityType.ShouldBe(typeof(VersionedOrder));
            exception.Key.ShouldBe(id);
            exception.ExpectedVersion.ShouldBe(0);
            exception.InnerException.ShouldBeOfType<DbUpdateConcurrencyException>();
            stale.Version.ShouldBe(0);
        }

        await using var verify = database.CreateContext();
        var stored = await verify.Set<VersionedOrder>().SingleAsync(candidate => candidate.Id == id);
        stored.Name.ShouldBe("winner");
        stored.Version.ShouldBe(1);
    }

    [Fact]
    public async Task DeleteAsync_Throws_WhenStoredVersionIsNewer()
    {
        await using var database = await SqliteDatabase<CallerVersionedDbContext>.CreateAsync();
        var id = Guid.NewGuid();
        await InsertAsync(database, id, "original");

        VersionedOrder stale;
        await using (var context = database.CreateContext())
        {
            stale = await context.Set<VersionedOrder>().SingleAsync(candidate => candidate.Id == id);
        }

        await using (var context = database.CreateContext())
        {
            var repository = new GenocsRepository<VersionedOrder, CallerVersionedDbContext>(context);
            var current = await context.Set<VersionedOrder>().SingleAsync(candidate => candidate.Id == id);
            current.Name = "kept";
            await repository.UpdateAsync(current);
        }

        await using (var context = database.CreateContext())
        {
            var repository = new GenocsRepository<VersionedOrder, CallerVersionedDbContext>(context);

            await Should.ThrowAsync<OptimisticConcurrencyException>(() => repository.DeleteAsync(stale));
        }

        await using var verify = database.CreateContext();
        var stored = await verify.Set<VersionedOrder>().SingleAsync(candidate => candidate.Id == id);
        stored.Name.ShouldBe("kept");
        stored.Version.ShouldBe(1);
    }

    [Fact]
    public async Task DeleteAsync_RemovesCurrentVersion()
    {
        await using var database = await SqliteDatabase<CallerVersionedDbContext>.CreateAsync();
        var id = Guid.NewGuid();
        await InsertAsync(database, id, "doomed");

        await using (var context = database.CreateContext())
        {
            var repository = new GenocsRepository<VersionedOrder, CallerVersionedDbContext>(context);
            var order = await context.Set<VersionedOrder>().SingleAsync(candidate => candidate.Id == id);

            await repository.DeleteAsync(order);
        }

        await using var verify = database.CreateContext();
        (await verify.Set<VersionedOrder>().SingleOrDefaultAsync(candidate => candidate.Id == id)).ShouldBeNull();
    }

    [Fact]
    public async Task UpdateAsync_AllowsLastWrite_WhenAggregateIsNotVersioned()
    {
        await using var database = await SqliteDatabase<CallerVersionedDbContext>.CreateAsync();
        var id = Guid.NewGuid();

        await using (var context = database.CreateContext())
        {
            var repository = new GenocsRepository<PlainOrder, CallerVersionedDbContext>(context);
            await repository.AddAsync(new PlainOrder { Id = id, Name = "original" });
        }

        PlainOrder first;
        await using (var context = database.CreateContext())
        {
            first = await context.Set<PlainOrder>().SingleAsync(candidate => candidate.Id == id);
        }

        await using (var context = database.CreateContext())
        {
            var repository = new GenocsRepository<PlainOrder, CallerVersionedDbContext>(context);
            var current = await context.Set<PlainOrder>().SingleAsync(candidate => candidate.Id == id);
            current.Name = "second";
            await repository.UpdateAsync(current);
        }

        await using (var context = database.CreateContext())
        {
            var repository = new GenocsRepository<PlainOrder, CallerVersionedDbContext>(context);
            first.Name = "third";
            await repository.UpdateAsync(first);
        }

        await using var verify = database.CreateContext();
        var stored = await verify.Set<PlainOrder>().SingleAsync(candidate => candidate.Id == id);
        stored.Name.ShouldBe("third");
    }

    [Fact]
    public async Task UpdateAsync_Throws_WhenVersionIsNotAConcurrencyToken()
    {
        await using var database = await SqliteDatabase<UnconfiguredVersionedDbContext>.CreateAsync();
        var id = Guid.NewGuid();

        await using (var context = database.CreateContext())
        {
            context.Set<VersionedOrder>().Add(new VersionedOrder { Id = id, Name = "original" });
            await context.SaveChangesAsync();
        }

        await using var update = database.CreateContext();
        var repository = new GenocsRepository<VersionedOrder, UnconfiguredVersionedDbContext>(update);
        var order = await update.Set<VersionedOrder>().SingleAsync(candidate => candidate.Id == id);
        order.Name = "changed";

        var exception = await Should.ThrowAsync<InvalidOperationException>(() => repository.UpdateAsync(order));
        exception.Message.ShouldContain(nameof(GenocsModelConfigurationBuilderExtensions.AddGenocsConventions));
    }

    [Fact]
    public async Task ApplicationDbContext_SaveChangesAsync_RejectsStaleVersion()
    {
        await using var database = await SqliteDatabase<ApplicationVersionedDbContext>.CreateAsync();
        var id = Guid.NewGuid();

        await using (var context = database.CreateContext())
        {
            context.Model.FindEntityType(typeof(VersionedOrder))!
                .FindProperty(nameof(IVersioned.Version))!
                .IsConcurrencyToken
                .ShouldBeTrue();

            context.Set<VersionedOrder>().Add(new VersionedOrder { Id = id, Name = "original" });
            await context.SaveChangesAsync();
        }

        VersionedOrder stale;
        await using (var context = database.CreateContext())
        {
            stale = await context.Set<VersionedOrder>().SingleAsync(candidate => candidate.Id == id);
        }

        await using (var context = database.CreateContext())
        {
            var current = await context.Set<VersionedOrder>().SingleAsync(candidate => candidate.Id == id);
            current.Name = "winner";
            await context.SaveChangesAsync();
            current.Version.ShouldBe(1);
        }

        await using (var context = database.CreateContext())
        {
            context.Set<VersionedOrder>().Update(stale);
            stale.Name = "loser";

            await Should.ThrowAsync<OptimisticConcurrencyException>(() => context.SaveChangesAsync());
        }

        await using var verify = database.CreateContext();
        var stored = await verify.Set<VersionedOrder>().SingleAsync(candidate => candidate.Id == id);
        stored.Name.ShouldBe("winner");
        stored.Version.ShouldBe(1);
    }

    private static async Task InsertAsync(SqliteDatabase<CallerVersionedDbContext> database, Guid id, string name)
    {
        await using var context = database.CreateContext();
        var repository = new GenocsRepository<VersionedOrder, CallerVersionedDbContext>(context);
        await repository.AddAsync(new VersionedOrder { Id = id, Name = name });
    }

    private sealed class SqliteDatabase<TContext> : IAsyncDisposable
        where TContext : DbContext
    {
        private readonly SqliteConnection _connection;
        private readonly DbContextOptions<TContext> _options;

        private SqliteDatabase(SqliteConnection connection, DbContextOptions<TContext> options)
        {
            _connection = connection;
            _options = options;
        }

        public static async Task<SqliteDatabase<TContext>> CreateAsync()
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            var options = new DbContextOptionsBuilder<TContext>().UseSqlite(connection).Options;
            var database = new SqliteDatabase<TContext>(connection, options);
            await using var context = database.CreateContext();
            await context.Database.EnsureCreatedAsync();
            return database;
        }

        public TContext CreateContext()
            => (TContext)Activator.CreateInstance(typeof(TContext), _options)!;

        public async ValueTask DisposeAsync()
            => await _connection.DisposeAsync();
    }
}

internal sealed class CallerVersionedDbContext : DbContext
{
    public CallerVersionedDbContext(DbContextOptions<CallerVersionedDbContext> options)
        : base(options)
    {
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder.AddGenocsConventions();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VersionedOrder>().Ignore(order => order.DomainEvents);
        modelBuilder.Entity<PlainOrder>().Ignore(order => order.DomainEvents);
    }
}

internal sealed class ApplicationVersionedDbContext : ApplicationDbContext
{
    public ApplicationVersionedDbContext(DbContextOptions<ApplicationVersionedDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VersionedOrder>().Ignore(order => order.DomainEvents);
    }
}

internal sealed class UnconfiguredVersionedDbContext : DbContext
{
    public UnconfiguredVersionedDbContext(DbContextOptions<UnconfiguredVersionedDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VersionedOrder>().Ignore(order => order.DomainEvents);
    }
}

internal sealed class VersionedOrder : IAggregateRoot<Guid>, IVersioned
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public long Version { get; set; }

    public IReadOnlyCollection<IEvent> DomainEvents => [];

    public void AddDomainEvent(IEvent @event)
    {
    }

    public void ClearDomainEvents()
    {
    }

    public bool IsTransient() => Id == Guid.Empty;
}

internal sealed class PlainOrder : IAggregateRoot<Guid>
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public IReadOnlyCollection<IEvent> DomainEvents => [];

    public void AddDomainEvent(IEvent @event)
    {
    }

    public void ClearDomainEvents()
    {
    }

    public bool IsTransient() => Id == Guid.Empty;
}
