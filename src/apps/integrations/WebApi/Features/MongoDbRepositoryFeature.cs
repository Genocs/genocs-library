using Genocs.Common.CQRS.Queries;
using Genocs.Library.Demo.Domain.Aggregates;
using Genocs.Persistence.MongoDB.Domain.Repositories;

namespace Genocs.Library.Demo.WebApi.Features;

public static class MongoDbRepositoryFeature
{
    public static IEndpointRouteBuilder MapMongoDbRepositoryFeature(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder mongoGroup = endpoints
            .MapGroup("/MongoDbRepository")
            .WithTags("MongoDbRepository");

        mongoGroup
            .MapGet(string.Empty, () => Results.Ok("MongoDbRepository"))
            .Produces<string>(StatusCodes.Status200OK);

        mongoGroup
            .MapPost("/user", PostUserAsync)
            .Produces<User>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status500InternalServerError);

        mongoGroup
            .MapGet("/user", GetUserAsync)
            .Produces<User>(StatusCodes.Status200OK);

        mongoGroup
            .MapGet("/users", GetUsersAsync)
            .Produces<PagedResult<User>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

        return endpoints;
    }

    private static async Task<IResult> PostUserAsync(IMongoRepository<User> userRepository, CancellationToken cancellationToken)
    {
        var user = new User(DefaultIdType.NewGuid().ToString(), DefaultIdType.NewGuid().ToString(), 21, "ITA");
        User entity = await userRepository.InsertAsync(user, cancellationToken);
        return Results.Ok(entity);
    }

    private static async Task<IResult> GetUserAsync(IMongoRepository<User> userRepository, CancellationToken cancellationToken)
    {
        User user = await userRepository.GetAsync(_ => true, cancellationToken);
        return Results.Ok(user);
    }

    private static async Task<IResult> GetUsersAsync(
        IMongoRepository<User> userRepository,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (page < 0 || pageSize < 0)
        {
            return Results.BadRequest("`page` and `pageSize` must be greater than 0.");
        }

        ActivityJournalRequest request = ActivityJournalRequest.FromRequest(page, pageSize);

        var users = await userRepository.BrowseAsync(x => x.Age == 21, request, cancellationToken);
        return Results.Ok(users);
    }

    private sealed record PagedResult<T>(
        int Page,
        int PageSize,
        int TotalCount,
        IReadOnlyCollection<T> Items);

    public class ActivityJournalRequest : PagedQueryBase
    {
        public static ActivityJournalRequest FromRequest(int pageIndex = 0, int pageSize = 10)
        {
            return new ActivityJournalRequest
            {
                Page = pageIndex,
                Results = pageSize
            };
        }
    }
}
