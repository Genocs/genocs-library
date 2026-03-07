using System.Data;
using System.Data.Common;
using Genocs.Library.Demo.WebApi.BookStore.Domain;
using Microsoft.EntityFrameworkCore;

namespace Genocs.Library.Demo.WebApi.BookStore.Data;

public static class BookStoreDatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();
        BookStoreDbContext dbContext = scope.ServiceProvider.GetRequiredService<BookStoreDbContext>();

        await BaselineLegacyEnsureCreatedDatabaseAsync(dbContext, cancellationToken);
        await dbContext.Database.MigrateAsync(cancellationToken);

        if (await dbContext.Authors.AnyAsync(cancellationToken) || await dbContext.Books.AnyAsync(cancellationToken))
        {
            return;
        }

        Author stephenKing = new()
        {
            FirstName = "Stephen",
            LastName = "King",
            Biography = "American author known for horror, supernatural fiction, suspense and fantasy novels.",
        };

        Author georgeMartin = new()
        {
            FirstName = "George R. R.",
            LastName = "Martin",
            Biography = "American novelist and short-story writer, author of A Song of Ice and Fire.",
        };

        Book theStand = new()
        {
            Title = "The Stand",
            Isbn = "978-0307743688",
            Price = 14.99m,
            PublishedOnUtc = new DateTime(1978, 10, 3, 0, 0, 0, DateTimeKind.Utc),
        };

        Book aGameOfThrones = new()
        {
            Title = "A Game of Thrones",
            Isbn = "978-0553593716",
            Price = 12.99m,
            PublishedOnUtc = new DateTime(1996, 8, 1, 0, 0, 0, DateTimeKind.Utc),
        };

        theStand.BookAuthors.Add(new BookAuthor { Author = stephenKing, Book = theStand });
        aGameOfThrones.BookAuthors.Add(new BookAuthor { Author = georgeMartin, Book = aGameOfThrones });

        dbContext.Authors.AddRange(stephenKing, georgeMartin);
        dbContext.Books.AddRange(theStand, aGameOfThrones);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static async Task BaselineLegacyEnsureCreatedDatabaseAsync(BookStoreDbContext dbContext, CancellationToken cancellationToken)
    {
        List<string> allMigrations = dbContext.Database.GetMigrations().ToList();
        if (allMigrations.Count == 0)
        {
            return;
        }

        List<string> appliedMigrations = dbContext.Database.GetAppliedMigrations().ToList();
        if (appliedMigrations.Count > 0)
        {
            return;
        }

        bool hasLegacySchema = await HasLegacyBookStoreTablesAsync(dbContext, cancellationToken);
        if (!hasLegacySchema)
        {
            return;
        }

        string initialMigrationId = allMigrations[0];
        string productVersion = dbContext.Model.GetProductVersion() ?? "10.0.3";

        await dbContext.Database.ExecuteSqlRawAsync(
            """
            IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
            BEGIN
                CREATE TABLE [__EFMigrationsHistory] (
                    [MigrationId] nvarchar(150) NOT NULL,
                    [ProductVersion] nvarchar(32) NOT NULL,
                    CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
                );
            END;
            """,
            cancellationToken);

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"""
            IF NOT EXISTS (
                SELECT 1
                FROM [__EFMigrationsHistory]
                WHERE [MigrationId] = {initialMigrationId})
            BEGIN
                INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
                VALUES ({initialMigrationId}, {productVersion});
            END;
            """,
            cancellationToken);
    }

    private static async Task<bool> HasLegacyBookStoreTablesAsync(BookStoreDbContext dbContext, CancellationToken cancellationToken)
    {
        DbConnection connection = dbContext.Database.GetDbConnection();
        bool shouldCloseConnection = connection.State != ConnectionState.Open;

        if (shouldCloseConnection)
        {
            await connection.OpenAsync(cancellationToken);
        }

        try
        {
            await using DbCommand command = connection.CreateCommand();
            command.CommandText =
                "SELECT CASE WHEN OBJECT_ID(N'[Authors]') IS NOT NULL " +
                "AND OBJECT_ID(N'[Books]') IS NOT NULL " +
                "AND OBJECT_ID(N'[BookAuthors]') IS NOT NULL THEN 1 ELSE 0 END";

            object? result = await command.ExecuteScalarAsync(cancellationToken);
            return result is not null && result != DBNull.Value && Convert.ToInt32(result) == 1;
        }
        finally
        {
            if (shouldCloseConnection)
            {
                await connection.CloseAsync();
            }
        }
    }
}
