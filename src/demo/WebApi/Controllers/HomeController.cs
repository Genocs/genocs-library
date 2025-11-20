using Genocs.Library.Demo.WebApi.Configurations;
using Microsoft.AspNetCore.Mvc;

namespace Genocs.Library.Demo.WebApi.Controllers;

[ApiController]
[Route("")]
public class HomeController(SecretOptions secretSettings) : ControllerBase
{
    public readonly SecretOptions _secretSettings = secretSettings ?? throw new ArgumentNullException(nameof(secretSettings));

    [HttpGet("secret")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult GetSecret()
        => Ok($"Read: {_secretSettings.Secret}");
}
