using System.Net;
using PactNet;

namespace Genocs.Library.Demo.ContractTests;

/// <summary>
/// Provides a suite of contract tests for the "Something API" using the Pact framework.
/// </summary>
/// <remarks>This class is responsible for defining and verifying Http interactions between the "Something API
/// Consumer" and the "Something API" provider. It uses the Pact V4 specification to ensure that the API consumer and
/// provider adhere to a shared contract. The tests simulate real-world API interactions and validate the responses
/// against the expected contract.  The class initializes a Pact builder with default or custom configurations for the
/// pact and log directories. Each test defines specific interactions, including request and response details, and
/// verifies the contract by executing the consumer code against a mock server.</remarks>
public class WebApiContractTests
{
    private readonly IPactBuilderV4 pactBuilder;

    public WebApiContractTests()
    {
        // Use default pact directory ..\..\pacts and default log
        // directory ..\..\logs
        var pact = Pact.V4("Something API Consumer", "Something API", new PactConfig());

        // or specify custom log and pact directories
        pact = Pact.V4("Something API Consumer", "Something API", new PactConfig
        {
            PactDir = $"{Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName}{Path.DirectorySeparatorChar}pacts"
        });

        // Initialize Rust backend
        this.pactBuilder = pact.WithHttpInteractions();
    }

    [Fact(Skip = "Skipping until the API is ready")]
    public async Task GetSomething_WhenTheTesterSomethingExists_ReturnsTheSomething()
    {
        // Arrange
        this.pactBuilder
            .UponReceiving("A GET request to retrieve the something")
                .Given("There is a something with id 'tester'")
                .WithRequest(HttpMethod.Get, "/somethings/tester")
                .WithHeader("Accept", "application/json")
            .WillRespond()
                .WithStatus(HttpStatusCode.OK)
                .WithHeader("Content-Type", "application/json; charset=utf-8")
                .WithJsonBody(new
                {
                    id = "tester",
                    firstName = "Totally",
                    lastName = "Awesome"
                });

        await this.pactBuilder.VerifyAsync(async ctx =>
        {
            //// Act
            //var client = new SomethingApiClient(ctx.MockServerUri);
            //var something = await client.GetSomething("tester");

            //// Assert
            //Assert.Equal("tester", something.Id);
        });
    }
}
