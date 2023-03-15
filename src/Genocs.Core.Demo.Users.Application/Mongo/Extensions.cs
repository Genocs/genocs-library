using Convey.Persistence.MongoDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Trill.Services.Users.Core.Mongo.Documents;

namespace Trill.Services.Users.Core.Mongo;

public static class Extensions
{
    public static IApplicationBuilder UseMongo(this IApplicationBuilder builder)
    {
        using var scope = builder.ApplicationServices.CreateScope();
        var users = scope.ServiceProvider.GetService<IMongoRepository<UserDocument, Guid>>().Collection;
        var userBuilder = Builders<UserDocument>.IndexKeys;
        Task.Run(async () => await users.Indexes.CreateManyAsync(
            new[]
            {
                new CreateIndexModel<UserDocument>(userBuilder.Ascending(i => i.Email),
                    new CreateIndexOptions
                    {
                        Unique = true
                    }),
                new CreateIndexModel<UserDocument>(userBuilder.Ascending(i => i.Name),
                    new CreateIndexOptions
                    {
                        Unique = true
                    })
            }));

        return builder;
    }
}