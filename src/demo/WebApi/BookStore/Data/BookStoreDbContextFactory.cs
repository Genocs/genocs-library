using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Genocs.Library.Demo.WebApi.BookStore.Data;

public sealed class BookStoreDbContextFactory : IDesignTimeDbContextFactory<BookStoreDbContext>
{
    public BookStoreDbContext CreateDbContext(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        string connectionString = configuration.GetConnectionString("BookStore")
            ?? "Server=(localdb)\\MSSQLLocalDB;Database=Genocs.BookStore.Demo;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        DbContextOptionsBuilder<BookStoreDbContext> optionsBuilder = new();
        optionsBuilder.UseSqlServer(connectionString);

        return new BookStoreDbContext(optionsBuilder.Options);
    }
}
