using Genocs.Identities.Application.Mongo.Documents;
using Genocs.Persistence.MongoDb.Domain.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Genocs.Identities.Application.Mongo;

public static class Extensions
{
    public static IApplicationBuilder UseMongo(this IApplicationBuilder builder)
    {
        using var scope = builder.ApplicationServices.CreateScope();
        var users = scope.ServiceProvider.GetService<IMongoDbBaseRepository<UserDocument, Guid>>()?.Collection;

        if (users is null)
        {
            return builder;
        }

        var userBuilder = Builders<UserDocument>.IndexKeys;

        Task.Run(async () => await users.Indexes.CreateManyAsync(
            new[]
            {
                new CreateIndexModel<UserDocument>(
                    userBuilder.Ascending(i => i.Email),
                    new CreateIndexOptions
                    {
                        Unique = true
                    }),
                new CreateIndexModel<UserDocument>(
                    userBuilder.Ascending(i => i.Name),
                    new CreateIndexOptions
                    {
                        Unique = true
                    })
            }));

        return builder;
    }
}