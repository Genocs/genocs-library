using System.Diagnostics;
using Genocs.Common.Domain.Entities;
using Genocs.Core.Domain.Repositories;
using Genocs.Persistence.MongoDB;
using Genocs.Persistence.MongoDB.Configurations;
using Genocs.Persistence.MongoDB.Domain.Repositories;
using Genocs.Persistence.MongoDB.Extensions;
using MongoDB.Driver;
using Testcontainers.MongoDb;

namespace Genocs.Persistence.MongoDB.ComponentTests;

public sealed class DriverRepositoryTests : IAsyncLifetime
{
    private static readonly object ConventionsLock = new();
    private static bool _conventionsRegistered;

    private readonly MongoDbContainer _mongoContainer = new MongoDbBuilder("mongo:7.0")
        .Build();

    private MongoBaseRepositoryOfType<DriverEntity, Guid> _repository = default!;

    public static bool IsDockerAvailable => DockerEnvironment.IsAvailable;

    public async ValueTask InitializeAsync()
    {
        EnsureMongoConventionsRegistered();

        await _mongoContainer.StartAsync();

        var mongoClient = new MongoClient(_mongoContainer.GetConnectionString());
        var database = mongoClient.GetDatabase($"genocs_component_tests_{Guid.NewGuid():N}");
        var provider = new TestMongoDatabaseProvider(mongoClient, database);

        _repository = new MongoBaseRepositoryOfType<DriverEntity, Guid>(provider);
    }

    private static void EnsureMongoConventionsRegistered()
    {
        if (_conventionsRegistered)
        {
            return;
        }

        lock (ConventionsLock)
        {
            if (_conventionsRegistered)
            {
                return;
            }

            // Align component test serialization behavior with runtime AddMongo defaults.
            ServiceCollectionExtensions.RegisterConventions(MongoGuidRepresentationMode.Standard);
            _conventionsRegistered = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
    }

    [Fact(Skip = "Requires Docker daemon to run MongoDB container.", SkipUnless = nameof(IsDockerAvailable))]
    public async Task AddAsync_ThenGetByIdAsync_PersistsDocumentInMongoDb()
    {
        var entity = new DriverEntity
        {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            Status = "Available",
        };

        await _repository.AddAsync(entity);

        DriverEntity reloaded = await _repository.GetByIdAsync(entity.Id);

        Assert.NotNull(reloaded);
        Assert.Equal(entity.Id, reloaded.Id);
        Assert.Equal("John Doe", reloaded.Name);
        Assert.Equal("Available", reloaded.Status);
    }

    [Fact(Skip = "Requires Docker daemon to run MongoDB container.", SkipUnless = nameof(IsDockerAvailable))]
    public async Task UpdateAndDelete_PersistsMutationAndRemovalInMongoDb()
    {
        var entity = new DriverEntity
        {
            Id = Guid.NewGuid(),
            Name = "Jane Doe",
            Status = "Busy",
        };

        await _repository.AddAsync(entity);

        entity.Status = "Available";
        _repository.Update(entity);

        DriverEntity updated = await _repository.GetByIdAsync(entity.Id);
        Assert.Equal("Available", updated.Status);

        _repository.Delete(entity.Id);

        DriverEntity? deleted = _repository.FirstOrDefault(entity.Id);
        Assert.Null(deleted);
    }

    [TableMapping("drivers_component")]
    private sealed class DriverEntity : IEntity<Guid>
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public bool IsTransient() => Id == Guid.Empty;
    }

    private sealed class TestMongoDatabaseProvider(IMongoClient mongoClient, IMongoDatabase database) : IMongoDatabaseProvider
    {
        public IMongoClient MongoClient { get; } = mongoClient;

        public IMongoDatabase Database { get; } = database;
    }

    private static class DockerEnvironment
    {
        public static bool IsAvailable { get; } = CheckDockerAvailability();

        private static bool CheckDockerAvailability()
        {
            try
            {
                using var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "docker",
                        Arguments = "info --format '{{.ServerVersion}}'",
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    },
                };

                if (!process.Start())
                {
                    return false;
                }

                if (!process.WaitForExit(5000))
                {
                    try
                    {
                        process.Kill(true);
                    }
                    catch
                    {
                        // Best effort: process may have already exited.
                    }

                    return false;
                }

                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }
}
