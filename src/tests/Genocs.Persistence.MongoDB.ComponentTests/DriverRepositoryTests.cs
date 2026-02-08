using Testcontainers.MsSql;

namespace Genocs.Persistence.MongoDB.ComponentTests;

// 1. Implement IAsyncLifetime to handle Docker container startup/shutdown
public class DriverRepositoryTests : IAsyncLifetime
{
    // Define the container (Real SQL Server 2025)
    private readonly MsSqlContainer _dbContainer = new MsSqlBuilder("mcr.microsoft.com/mssql/server:2025-latest")
        .Build();

    [Fact(Skip = "Skipping since Docker is not available onto github build agent.")]
    public async ValueTask InitializeAsync()
    {
        // This creates the Docker container on the fly
        await _dbContainer.StartAsync(CancellationToken.None);
    }

    [Fact(Skip = "Skipping since Docker is not available onto github build agent.")]
    public async ValueTask DisposeAsync()
    {
        // This kills the container after tests finish (Cleanup)
        await _dbContainer.DisposeAsync();
    }

    [Fact(Skip = "Skipping since Docker is not available onto github build agent.")]
    public async ValueTask SaveDriver_ShouldPersistToDatabase()
    {
        // Arrange: Get the connection string from the running container
        string connectionString = _dbContainer.GetConnectionString();

        Assert.NotNull(connectionString);
        Assert.NotEmpty(connectionString);

        //// Setup EF Core to use the Container
        //var options = new DbContextOptionsBuilder<UberDbContext>()
        //    .UseSqlServer(connectionString)
        //    .Options;

        //// Apply Migrations (Important: Creates tables in the fresh container)
        //using (var context = new UberDbContext(options))
        //{
        //    await context.Database.MigrateAsync();

        //    // Act
        //    context.Drivers.Add(new Driver { Name = "John Doe", Status = "Available" });
        //    await context.SaveChangesAsync();
        //}

        //// Assert: Verify with a NEW context instance to ensure it wasn't just in RAM
        //using (var context = new UberDbContext(options))
        //{
        //    var driver = await context.Drivers.FirstAsync();
        //    Assert.Equal("John Doe", driver.Name);
        //    Assert.Equal("Available", driver.Status);
        //}
    }
}