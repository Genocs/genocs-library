using Genocs.Core.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;

namespace Genocs.Core.Demo.WebApi.Controllers;

[ApiController]
[Route("")]
public class HomeController : ControllerBase
{

    public HomeController() { }

    [HttpGet]
    public IActionResult Get()
        => Ok("Genocs.Core.Demo.WebApi");

    [HttpGet("ping")]
    public IActionResult Ping()
        => Ok("pong");

}
