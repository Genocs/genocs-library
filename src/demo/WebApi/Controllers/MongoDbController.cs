using Genocs.Core.Demo.Domain.Aggregates;
using Genocs.Persistence.MongoDb.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Genocs.Library.Demo.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MongoDbRepositoryController : ControllerBase
{
    private readonly IMongoDbRepository<User> _userRepository;

    private readonly ILogger<MongoDbRepositoryController> _logger;

    public MongoDbRepositoryController(ILogger<MongoDbRepositoryController> logger, IMongoDbRepository<User> userRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
    }

    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult Get()
        => Ok("MongoDbRepositoryController");

    [HttpPost("user")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostUserAsync()
    {
        var user = new User(DefaultIdType.NewGuid().ToString(), DefaultIdType.NewGuid().ToString(), 21, "ITA");

        var objectId = await _userRepository.InsertAsync(user);
        return Ok(objectId.ToString());
    }

    [HttpGet("user")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserAsync()
    {
        var user = await _userRepository.GetAsync(c => true);
        return Ok(user);
    }
}
