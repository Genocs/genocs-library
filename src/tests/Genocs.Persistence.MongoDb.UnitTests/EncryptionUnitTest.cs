using Microsoft.Extensions.Configuration;

namespace Genocs.Persistence.MongoDb.UnitTests;

public class EncryptionUnitTest
{
    public static IConfiguration InitConfiguration()
    {
        var config = new ConfigurationBuilder()
           .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();
        return config;
    }
}
