using Genocs.Core.Builders;
using Genocs.Persistence.CosmosDB.Configurations;
using Genocs.Persistence.CosmosDB.Domain.Entities;
using Genocs.Persistence.CosmosDB.Domain.Repositories;
using Genocs.Persistence.CosmosDB.Serialization;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.Persistence.CosmosDB.Extensions;

/// <summary>
/// Registers the Cosmos client and container repositories on <see cref="IGenocsBuilder"/>.
/// </summary>
public static class CosmosExtensions
{
    /// <summary>
    /// Registers <see cref="CosmosClient"/> and <see cref="Database"/> from configuration.
    /// Does nothing when the section is missing.
    /// </summary>
    /// <param name="builder">Genocs builder.</param>
    /// <param name="sectionName">Configuration section. Defaults to <see cref="CosmosOptions.Position"/>.</param>
    /// <returns>The same builder.</returns>
    public static IGenocsBuilder AddCosmos(this IGenocsBuilder builder, string sectionName = CosmosOptions.Position)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = CosmosOptions.Position;
        }

        IConfigurationSection? section = builder.Configuration?.GetSection(sectionName);
        if (section is null || !section.Exists())
        {
            return builder;
        }

        builder.Services.Configure<CosmosOptions>(section);
        CosmosOptions options = builder.GetOptions<CosmosOptions>(sectionName);
        return builder.AddCosmos(options);
    }

    /// <summary>
    /// Registers <see cref="CosmosClient"/> and <see cref="Database"/> from the supplied options.
    /// Does nothing when the options are invalid. Call <see cref="AddCosmosRepository{TEntity,TKey}"/> for each container.
    /// </summary>
    /// <param name="builder">Genocs builder.</param>
    /// <param name="options">Connection options.</param>
    /// <returns>The same builder.</returns>
    public static IGenocsBuilder AddCosmos(this IGenocsBuilder builder, CosmosOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        if (!builder.TryRegister(CosmosOptions.Position))
        {
            return builder;
        }

        if (!CosmosOptions.IsValid(options))
        {
            return builder;
        }

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton(sp =>
        {
            CosmosOptions resolved = sp.GetRequiredService<CosmosOptions>();
            return new CosmosClient(
                resolved.ConnectionString,
                new CosmosClientOptions
                {
                    ApplicationName = "Genocs",
                    Serializer = new CosmosSystemTextJsonSerializer(),
                });
        });

        builder.Services.AddSingleton(sp =>
        {
            CosmosOptions resolved = sp.GetRequiredService<CosmosOptions>();
            CosmosClient client = sp.GetRequiredService<CosmosClient>();
            return client.GetDatabase(resolved.Database);
        });

        return builder;
    }

    /// <summary>
    /// Registers <see cref="ICosmosRepository{TEntity,TKey}"/> for one container.
    /// The container must already exist and use partition key path <c>/partitionKey</c>.
    /// </summary>
    /// <typeparam name="TEntity">Document type.</typeparam>
    /// <typeparam name="TKey">Document id type.</typeparam>
    /// <param name="builder">Genocs builder.</param>
    /// <param name="containerName">Container id.</param>
    /// <returns>The same builder.</returns>
    public static IGenocsBuilder AddCosmosRepository<TEntity, TKey>(this IGenocsBuilder builder, string containerName)
        where TEntity : class, ICosmosEntity<TKey>
    {
        ArgumentNullException.ThrowIfNull(builder);
        if (string.IsNullOrWhiteSpace(containerName))
        {
            throw new ArgumentException("Cosmos container name is required.", nameof(containerName));
        }

        CosmosItemIdentity.EnsureSupportedKey<TKey>();

        builder.Services.AddTransient<ICosmosRepository<TEntity, TKey>>(sp =>
        {
            Database database = sp.GetRequiredService<Database>();
            return new CosmosRepository<TEntity, TKey>(database.GetContainer(containerName));
        });

        return builder;
    }
}
