using Genocs.Core.Builders;
using Genocs.Library.Demo.WebApi.Services;
using Genocs.Saga;
using Genocs.Saga.Integrations.Redis;

namespace Genocs.Library.Demo.WebApi.Extensions;

public static class SagaExtensions
{
    public static IGenocsBuilder AddSagaServices(this IGenocsBuilder builder)
    {
        builder.Services.AddScoped<ISagaTransactionService, SagaTransactionService>();

        builder.Services.AddSaga(x => x.UseRedisPersistence(builder.Configuration!, "redis"));
        return builder;
    }
}
