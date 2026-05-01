using Genocs.Core.Builders;
using Genocs.Library.Demo.Domain.EFCore.BookStore.Data;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Library.Demo.WebApi.Extensions;

public static class EntityFrameworkExtensions
{
    public static IGenocsBuilder AddBookStoreDbContext(this IGenocsBuilder builder)
    {
        string bookStoreConnectionString = builder?.Configuration?.GetConnectionString("BookStore")
            ?? throw new InvalidOperationException("Missing 'ConnectionStrings:BookStore' configuration.");

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
