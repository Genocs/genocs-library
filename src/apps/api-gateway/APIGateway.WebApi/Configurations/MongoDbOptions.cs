namespace Genocs.APIGateway.Configurations;

public class MongoDbOptions
{
    /// <summary>
    /// Default section name.
    /// </summary>
    public const string Position = "yarp_mongodb";

    public string RoutesCollection { get; set; } = "yarp_routes";

    public string? ClustersCollection { get; set; } = "yarp_clusters";
}