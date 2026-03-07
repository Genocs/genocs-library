using Genocs.Library.Demo.WebApi.Services;
using Genocs.Saga;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Genocs.Library.Demo.WebApi.Features;

public static class SagaFeature
{
    public static IEndpointRouteBuilder MapSagaFeature(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/saga/start", async (ISagaTransactionService sagaService) =>
        {
            SagaId sagaId = await sagaService.StartTransactionAsync("Start transaction", "Test");
            return Results.Ok(sagaId);
        }).WithTags("Saga");

        endpoints.MapPost("/saga/complete/{sagaId}", async (ISagaTransactionService sagaService, string sagaId) =>
        {
            SagaId completedSagaId = await sagaService.CompleteTransactionAsync(sagaId, "Complete transaction", "Test");
            return Results.Ok(completedSagaId);
        }).WithTags("Saga");

        return endpoints;
    }
}
