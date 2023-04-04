using Microsoft.AspNetCore.Mvc;

namespace Genocs.SignalR.WebApi.Controllers;

[Route("")]
public class HomeController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() 
        => Ok("Genocs SignalR Service");
}