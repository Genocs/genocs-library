using Genocs.Library.Demo.Saga.WebApi.Sagas;
using Genocs.Saga;

namespace Genocs.Library.Demo.Saga.WebApi.Extensions;

public static class SagaExtensions
{
    public static void ConfigureSaga(this WebApplication app)
    {
        // Resolve the coordinator and set up simple saga
        var coordinator = app.Services.GetService<ISagaCoordinator>();

        // get logger
        var logger = app.Services.GetService<ILogger<SampleSaga>>();

        var context = SagaContext
            .Create()
            .WithSagaId(SagaId.NewSagaId())
            .WithOriginator("Test")
            .WithMetadata("key", "lulz")
            .Build();

        var context2 = SagaContext
            .Create()
            .WithSagaId(SagaId.NewSagaId())
            .WithOriginator("Test")
            .WithMetadata("key", "lulz")
            .Build();

        // Process some messages to see how it works
        // TODO: Move to a controller and make it more interactive
        coordinator.ProcessAsync(new Message1 { Text = "This message will be used one day..." }, context);

        _ = coordinator.ProcessAsync(
            new Message2 { Text = "But this one will be printed first! (We compensate from the end to beggining of the log)" },
            onCompleted: (m, ctx) =>
            {
                logger?.LogInformation(
                    "Saga completed successfully with message: {MessageText} and metadata: {Metadata}",
                    m.Text,
                    ctx.Metadata.Select(md => $"{md.Key}={md.Value}").ToArray());

                return Task.CompletedTask;
            },
            onRejected: (m, ctx) =>
            {
                logger?.LogError(
                    "Saga rejected with message: {MessageText} and metadata: {Metadata}",
                    m.Text,
                    ctx.Metadata.Select(md => $"{md.Key}={md.Value}").ToArray());

                return Task.CompletedTask;
            },
            context: context);
    }
}
