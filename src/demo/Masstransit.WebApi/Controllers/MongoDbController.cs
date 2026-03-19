using System.Net.Mime;
using Genocs.Core.Demo.Domain.Aggregates;
using Genocs.Persistence.MongoDB.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Genocs.Library.Demo.Masstransit.WebApi.Controllers;

[ApiController]
[Route("[controller]")]
public class MongoDbRepositoryController(IMongoRepository<User> userRepository) : ControllerBase
{
    private readonly IMongoRepository<User> _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));

    /// <summary>
    /// The Get method is a simple endpoint that returns a string message indicating the name of the controller.
    /// It serves as a basic test to confirm that the controller is set up correctly and can respond to Http GET requests.
    /// </summary>
    /// <returns>A string message indicating the name of the controller.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    public IActionResult Get()
        => Ok(nameof(MongoDbRepositoryController));

    /// <summary>
    /// This is a demo method to test the MongoDbRepository.
    /// It will create a new user and insert it into the database, then return the object just inserted.
    /// </summary>
    /// <remarks>Ensure that the MongoDB connection string and database name are correctly
    /// configured in the application settings before calling this method.
    /// The user object is created with random values for demonstration purposes,
    /// and the inserted entity is returned in the response.</remarks>
    /// <returns>The object id of the inserted user.</returns>
    [HttpPost("user")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PostUserAsync(CancellationToken cancellationToken)
    {
        var user = new User(DefaultIdType.NewGuid().ToString(), DefaultIdType.NewGuid().ToString(), 21, "ITA");

        var entity = await _userRepository.InsertAsync(user, cancellationToken);
        return Ok(entity);
    }

    /// <summary>
    /// Retrieves the user details asynchronously.
    /// </summary>
    /// <remarks>This method fetches the user data from the repository without applying any filters. Ensure
    /// that the user repository is properly initialized before calling this method.</remarks>
    /// <returns>An <see cref="IActionResult"/> that contains the user information. Returns a 200 OK response with the user data
    /// if found.</returns>
    [HttpGet("user")]
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserAsync(CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetAsync(_ => true, cancellationToken);
        return Ok(user);
    }
}
