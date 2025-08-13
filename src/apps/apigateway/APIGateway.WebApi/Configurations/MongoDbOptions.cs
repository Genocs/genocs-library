using Genocs.Persistence.MongoDb.Configurations;

namespace Genocs.APIGateway.WebApi.Configurations;

public class YarpMongoDbOptions : MongoDbOptions
{
    public string RoutesCollection { get; set; } = "yarp_routes";

    public string? ClustersCollection { get; set; } = "yarp_clusters";
}