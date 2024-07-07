using Genocs.Core.Demo.WebApi.Options;
using Microsoft.AspNetCore.Mvc;

namespace Genocs.Core.Demo.WebApi.Controllers;

[ApiController]
[Route("")]
public class HomeController : ControllerBase
{
    public readonly SecretOptions _secretSettings;

    public HomeController(SecretOptions secretSettings)
    {
        _secretSettings = secretSettings ?? throw new ArgumentNullException(nameof(secretSettings));
    }

    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult Get()
        => Ok("Genocs Demo WebApi");

    [HttpGet("ping")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult Ping()
        => Ok("pong");

    [HttpGet("secret")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult GetSecret()
        => Ok($"Read: {_secretSettings.Secret}");
}
