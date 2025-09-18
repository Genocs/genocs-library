﻿using Genocs.Core.Demo.Domain.Aggregates;
using Genocs.Persistence.MongoDb.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Genocs.Core.Demo.WebApi.Controllers;

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

    [HttpPost("dummy")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public async Task<IActionResult> PostDummy()
    {
        User user = new User(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), 21, "ITA");

        User objectId = await _userRepository.InsertAsync(user);
        return Ok(objectId.ToString());
    }
}
