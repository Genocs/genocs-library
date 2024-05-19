using Genocs.Core.Demo.WebApi.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Genocs.Core.Demo.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ExternalApiController : ControllerBase
{
    private readonly ILogger<DemoController> _logger;
    private readonly IExternalServiceClient _externalServiceClient;

    public ExternalApiController(ILogger<DemoController> logger, IExternalServiceClient externalServiceClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _externalServiceClient = externalServiceClient ?? throw new ArgumentNullException(nameof(externalServiceClient));
    }

    [HttpGet("")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> HomeAsync()
        => await Task.Run(() => Ok("done"));

    [HttpPost("")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IssuingResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostIssueAsync(IssuingRequest request)
        => Ok(await _externalServiceClient.IssueAsync(request));
}
