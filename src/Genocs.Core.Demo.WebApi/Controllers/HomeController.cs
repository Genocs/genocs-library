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

public class Product
{
    public int Count { get; set; }
    public decimal Price { get; set; }
}

public class NewRedeemReqEvent : IEvent
{
    public string RequestId { get; set; }
    public string UserId { get; set; }
    public DateTime TimeStamp { get; set; }
    public string CardToken { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; }
}
