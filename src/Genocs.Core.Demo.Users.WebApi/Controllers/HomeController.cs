using Microsoft.AspNetCore.Mvc;

namespace Genocs.Core.Demo.WebApi.Controllers;

[ApiController]
[Route("")]
public class HomeController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult Get()
        => Ok("Genocs.Core.Demo.Users.WebApi");

    [HttpGet("ping")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult Ping()
        => Ok("pong");
}
