using Genocs.Core.Demo.WebApi.Infrastructure.Services;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace Genocs.Core.Demo.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class ExternalApiController : ControllerBase
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<DemoController> _logger;
    private readonly IExternalServiceClient _externalServiceClient;

    public ExternalApiController(ILogger<DemoController> logger, IPublishEndpoint publishEndpoint, IExternalServiceClient externalServiceClient)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _publishEndpoint = publishEndpoint ?? throw new ArgumentNullException(nameof(publishEndpoint));
        _externalServiceClient = externalServiceClient ?? throw new ArgumentNullException(nameof(externalServiceClient));
    }

    [HttpGet("")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(IssuingResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> HomeAsync()
    {
        var request = new IssuingRequest
        {
            ExternalReference = Guid.NewGuid().ToString(),
            Currency = "xxxx",
            Amount = 100,
            PartnerCode = "xxxx"
        };

        var response = await _externalServiceClient.IssueAsync(request);

        return Ok(response);
    }
}
