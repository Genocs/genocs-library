﻿using Genocs.APIGateway.WebApi.Configurations;
using Genocs.Persistence.MongoDb;
using Microsoft.Extensions.Primitives;
using MongoDB.Driver;
using Yarp.ReverseProxy.Configuration;

namespace Genocs.APIGateway.WebApi.Providers;

/// <summary>
/// Provides an implementation of IProxyConfigProvider to support config read from MongoDB.
/// </summary>
public class MongodbConfigProvider : IMongodbConfigProvider
{
    // Marked as volatile so that updates are atomic
    private InMemoryConfig _config;

    private IMongoDatabaseProvider _mongoDbDatabase;

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public MongodbConfigProvider(ILogger<MongodbConfigProvider> logger, IMongoDatabaseProvider mongoDbDatabase, YarpMongoDbOptions options)
    {
        _mongoDbDatabase = mongoDbDatabase;
        var routes = _mongoDbDatabase.Database.GetCollection<RouteConfig>(options.RoutesCollection).Find(c => true).ToList();
        var clusters = _mongoDbDatabase.Database.GetCollection<ClusterConfig>(options.ClustersCollection).Find(c => true).ToList();
        _config = new InMemoryConfig(routes, clusters, DefaultIdType.NewGuid().ToString());
    }

    /// <summary>
    /// Creates a new instance.
    /// </summary>
    public MongodbConfigProvider(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
        : this(routes, clusters, DefaultIdType.NewGuid().ToString())
    {
    }

    /// <summary>
    /// Creates a new instance, specifying a revision id of the configuration.
    /// </summary>
    public MongodbConfigProvider(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters, string revisionId)
    {
        _config = new InMemoryConfig(routes, clusters, revisionId);
    }

    /// <summary>
    /// Swaps the config state with a new snapshot of the configuration, then signals that the old one is outdated.
    /// </summary>
    public void Update(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
    {
        var newConfig = new InMemoryConfig(routes, clusters);
        UpdateInternal(newConfig);
    }

    /// <summary>
    /// Swaps the config state with a new snapshot of the configuration, then signals that the old one is outdated.
    /// </summary>
    public void Update(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters, string revisionId)
    {
        var newConfig = new InMemoryConfig(routes, clusters, revisionId);
        UpdateInternal(newConfig);
    }

    private void UpdateInternal(InMemoryConfig newConfig)
    {
        var oldConfig = Interlocked.Exchange(ref _config, newConfig);
        oldConfig.SignalChange();
    }

    /// <summary>
    /// Implementation of the IProxyConfigProvider.GetConfig method to supply the current snapshot of configuration.
    /// </summary>
    /// <returns>An immutable snapshot of the current configuration state.</returns>
    public IProxyConfig GetConfig()
        => _config;

    /// <summary>
    /// Implementation of IProxyConfig which is a snapshot of the current config state. The data for this class should be immutable.
    /// </summary>
    private class InMemoryConfig : IProxyConfig
    {
        // Used to implement the change token for the state
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public InMemoryConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters)
            : this(routes, clusters, DefaultIdType.NewGuid().ToString())
        {
        }

        public InMemoryConfig(IReadOnlyList<RouteConfig> routes, IReadOnlyList<ClusterConfig> clusters, string revisionId)
        {
            RevisionId = revisionId ?? throw new ArgumentNullException(nameof(revisionId));
            Routes = routes;
            Clusters = clusters;
            ChangeToken = new CancellationChangeToken(_cts.Token);
        }

        /// <inheritdoc/>
        public string RevisionId { get; }

        /// <summary>
        /// A snapshot of the list of routes for the proxy.
        /// </summary>
        public IReadOnlyList<RouteConfig> Routes { get; }

        /// <summary>
        /// A snapshot of the list of Clusters which are collections of interchangeable destination endpoints.
        /// </summary>
        public IReadOnlyList<ClusterConfig> Clusters { get; }

        /// <summary>
        /// Fired to indicate the proxy state has changed, and that this snapshot is now stale.
        /// </summary>
        public IChangeToken ChangeToken { get; }

        internal void SignalChange()
        {
            _cts.Cancel();
        }
    }
}