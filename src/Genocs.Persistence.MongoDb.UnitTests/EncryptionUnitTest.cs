using Genocs.Persistence.MongoDb.Encryptions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

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

    //[Fact]
    //public void CreateAzureInitializerTest()
    //{
    //    MongoDbEncryptionSettings mongoDbEncryptionSettings = new MongoDbEncryptionSettings()
    //    {
    //        ClientId = "ed9fb3d9-fbf3-4149-9163-1efb74fa9064",
    //        ClientSecret = "Mongo-Encrypt",
    //        ConnectionString = "mongodb://localhost:27017",
    //        KeyName = "Mongo-Encrypt",
    //        KeyVaultEndpoint = "https://kv-genocs.vault.azure.net/",
    //        KeyVersion = "9d4667c4f8a245229425782e5090ff9d",
    //        LibPath = "lib",
    //        TenantId = "2448c491-9325-42f1-8b00-7b041b2361ce"
    //    };

    //    // Make sure you include using Moq;
    //    var mock = new Mock<IOptions<MongoDbEncryptionSettings>>();
    //    // We need to set the Value of IOptions to be the SampleOptions Class
    //    mock.Setup(ap => ap.Value).Returns(mongoDbEncryptionSettings);


    //    AzureInitializer initializer = new AzureInitializer();
    //    IOptions<MongoDbEncryptionSettings> options = Microsoft.Extensions.Options.Options.Create<MongoDbEncryptionSettings>(mongoDbEncryptionSettings);

    //    var res = initializer.EncryptionOptions(options);

    //    Assert.NotNull(res);
    //}
}
