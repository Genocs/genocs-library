using Genocs.Core.Builders;
using Genocs.Library.Demo.Domain.EFCore.BookStore.Data;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Library.Demo.WebApi.Extensions;

public static class EntityFrameworkExtensions
{
    /// <summary>
    /// The AddBookStoreDbContext method registers the BookStoreDbContext with the dependency injection container,
    /// configuring it to use SQL Server with the connection string specified in the application's configuration.
    /// It also specifies the assembly where EF Core migrations are located.
    /// This setup allows the application to interact with the BookStore database using Entity Framework Core.
    /// </summary>
    /// <param name="builder">The IGenocsBuilder instance used to configure services.</param>
    /// <returns>The updated IGenocsBuilder instance.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the BookStore connection string is missing in the configuration.</exception>
    public static IGenocsBuilder AddBookStoreDbContext(this IGenocsBuilder builder)
    {
        // NOTE: This is a work in progress and may require adjustments based on the actual structure of the IGenocsBuilder and how it manages configuration and services.
        string bookStoreConnectionString = builder?.Configuration?.GetConnectionString("BookStore")
            ?? throw new InvalidOperationException("Missing 'ConnectionStrings:BookStore' configuration.");

        // TODO: Use a more robust approach to handle migrations assembly, possibly by using a constant or configuration
        // value instead of hardcoding the assembly name.
        builder.Services.AddDbContext<BookStoreDbContext>(options =>
            options.UseSqlServer(bookStoreConnectionString, b => b.MigrationsAssembly("Genocs.Library.Demo.Domain.EFCore")));

        return builder;
    }

    public static async Task<WebApplication> UseBookStoreDbContextAsync(this WebApplication application)
    {
        await BookStoreDatabaseInitializer.InitializeAsync(application.Services);
        return application;
    }
}
