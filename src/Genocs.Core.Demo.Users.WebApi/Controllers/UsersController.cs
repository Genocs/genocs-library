using Microsoft.AspNetCore.Mvc;

namespace Genocs.Core.Demo.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult Get()
        => Ok("Users.Controller");
}
