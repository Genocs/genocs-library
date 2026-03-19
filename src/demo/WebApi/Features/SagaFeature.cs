using Genocs.Library.Demo.WebApi.Sagas;
using Genocs.Library.Demo.WebApi.Services;
using Genocs.Saga;
using Microsoft.AspNetCore.Mvc;

namespace Genocs.Library.Demo.WebApi.Features;

public static class SagaFeature
{
    public static IEndpointRouteBuilder MapSagaFeature(this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder sagaGroup = endpoints.MapGroup("/saga").WithTags("Saga");

        sagaGroup.MapPost("/start", StartTransactionAsync);
        sagaGroup.MapPost("/complete/{sagaId}", CompleteTransactionAsync);

        return endpoints;
    }

    private static async Task<IResult> StartTransactionAsync(ISagaTransactionService sagaService, [FromBody] StartSagaCommand command)
    {
        SagaId sagaId = await sagaService.StartTransactionAsync(command, "Test");
        return Results.Ok(sagaId);
    }

    private static async Task<IResult> CompleteTransactionAsync(ISagaTransactionService sagaService, string sagaId)
    {
        SagaId completedSagaId = await sagaService.CompleteTransactionAsync(sagaId, "Complete transaction", "Test");
        return Results.Ok(completedSagaId);
    }
}
