using Genocs.Persistence.MongoDB.Configurations;

namespace Genocs.APIGateway.WebApi.Configurations;

public class YarpMongoDbOptions : MongoOptions
{
    public string RoutesCollection { get; set; } = "yarp_routes";

    public string? ClustersCollection { get; set; } = "yarp_clusters";
}