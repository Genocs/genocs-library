using Genocs.Saga.Builders;
using Shouldly;
using Xunit;

namespace Genocs.Saga.UnitTests.Builders;

public class SagaContextBuilderTests
{
    [Fact]
    public void WithSagaId_Throws_InvalidOperationException_When_Originator_Is_Null()
    {
        Should.Throw<InvalidOperationException>(
            () =>
            {
                var sagaId = SagaId.NewSagaId();

                var context = _builder
                    .WithSagaId(sagaId)
                    .Build();
            },
            "Originator must be provided.");
    }

    [Fact]
    public void WithOriginator_Throws_InvalidOperationException_When_SagaId_Is_Null()
    {
        Should.Throw<InvalidOperationException>(
        () =>
        {
            const string originator = "originator";

            var context = _builder
                .WithOriginator(originator)
                .Build();
        },
        "SagaId must be provided.");
    }

    [Fact]
    public void WithMetadata_Adds_SagaContextMetadata_To_List_With_Given_Values()
    {
        string key = "key";
        string value = "value";
        var sagaId = SagaId.NewSagaId();
        const string originator = "originator";

        var context = _builder
            .WithSagaId(sagaId)
            .WithOriginator(originator)
            .WithMetadata(key, value)
            .Build();

        context.Metadata.Count.ShouldBe(1);
        context.Metadata.First().Key.ShouldBe(key);
        context.Metadata.First().Value.ShouldBe(value);
    }

    #region ARRANGE

    private readonly ISagaContextBuilder _builder;

    public SagaContextBuilderTests()
    {
        _builder = new SagaContextBuilder();
    }

    #endregion
}
