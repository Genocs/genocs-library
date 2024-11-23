using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Genocs.Core.Demo.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize]
public class AuthorizedController : ControllerBase
{
    private readonly ILogger<AuthorizedController> _logger;

    public AuthorizedController(ILogger<AuthorizedController> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpGet("GetAuthorized")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostSubmitDemoCommand()
    {
        return await Task.Run(() => Ok($"Done! Authorization is: {HttpContext.Request.Headers["Authorization"]}"));
    }
}
