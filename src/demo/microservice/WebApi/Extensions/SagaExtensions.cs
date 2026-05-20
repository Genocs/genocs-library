using Genocs.Core.Builders;
using Genocs.Library.Demo.WebApi.Services;
using Genocs.Saga;
using Genocs.Saga.Integrations.MongoDB;

namespace Genocs.Library.Demo.WebApi.Extensions;

public static class SagaExtensions
{
    /// <summary>
    /// This extension method scaffold Genocs Saga along with MongoDB persistence.
    /// This step require to have MongoDB persistence up and running.
    /// No action are required on your side. The library is taking care about the
    /// setup avoiding double registration in case you need the same persistance
    /// layer for both 'Business Object' and 'Saga' support
    /// The Genocs Saga persistence can be acieve with Resis as well.
    /// </summary>
    /// <param name="builder">The genocs builder.</param>
    /// <returns>The genocs builder to be used for chaining.</returns>
    public static IGenocsBuilder AddApplicationSaga(this IGenocsBuilder builder)
    {
        builder.Services.AddScoped<ISagaTransactionService, SagaTransactionService>();

        builder.Services.AddSaga(x => x.UseMongoPersistence(builder.Configuration!));
        return builder;
    }
}
