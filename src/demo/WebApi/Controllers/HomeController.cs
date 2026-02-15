using Genocs.Library.Demo.WebApi.Configurations;
using Microsoft.AspNetCore.Mvc;

namespace Genocs.Library.Demo.WebApi.Controllers;

[ApiController]
[Route("")]
public class HomeController(SecretOptions secretSettings) : ControllerBase
{
    public readonly SecretOptions _secretSettings = secretSettings ?? throw new ArgumentNullException(nameof(secretSettings));

    /// <summary>
    /// The Get method is a simple endpoint that returns a welcome message indicating that the Genocs Library Demo Web API is running.
    /// It serves as a basic test to confirm that the controller is set up correctly and can respond to HTTP GET requests.
    /// </summary>
    /// <returns>A string message indicating that the Genocs Library Demo Web API is running.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult Get()
        => Ok("Welcome to Genocs Library Demo Web API!");

    /// <summary>
    /// Retrieves the secret value from the application's configuration settings. Or from a secret store like Azure Key Vault, depending on how the SecretOptions is configured.
    /// </summary>
    /// <remarks>This endpoint is intended only for demonstration purposes. Please do not expose sensitive information in a production environment.</remarks>
    /// <returns>An HTTP 200 OK response containing the secret value as a string in the response body.</returns>
    [HttpGet("secret")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult GetSecret()
        => Ok($"Read: {_secretSettings.Secret}");
}
