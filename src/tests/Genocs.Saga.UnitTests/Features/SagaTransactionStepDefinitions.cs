using Genocs.Library.Demo.HelloWorld.WebApi.Sagas;
using Genocs.Library.Demo.HelloWorld.WebApi.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Reqnroll;
using Xunit;

namespace Genocs.Saga.UnitTests.Features;

[Binding]
public class SagaTransactionStepDefinitions
{
    private readonly Mock<ISagaCoordinator> _mockSagaCoordinator;
    private readonly Mock<ILogger<SagaTransactionService>> _mockLogger;
    private ISagaTransactionService _sagaTransactionService;

    private string _transactionText;
    private string _originator;
    private SagaId _resultSagaId;
    private string _existingSagaId;
    private ISagaContext _capturedContext;

    public SagaTransactionStepDefinitions()
    {
        _mockSagaCoordinator = new Mock<ISagaCoordinator>();
        _mockLogger = new Mock<ILogger<SagaTransactionService>>();
    }

    [Given(@"the saga transaction service is initialized")]
    public void GivenTheSagaTransactionServiceIsInitialized()
    {
        _sagaTransactionService = new SagaTransactionService(
            _mockSagaCoordinator.Object,
            _mockLogger.Object);
    }

    [Given(@"I have transaction details with text ""(.*)"" and originator ""(.*)""")]
    public void GivenIHaveTransactionDetailsWithTextAndOriginator(string text, string originator)
    {
        _transactionText = text;
        _originator = originator;
    }

    [Given(@"I have started a saga transaction with text ""(.*)"" and originator ""(.*)""")]
    public async Task GivenIHaveStartedASagaTransactionWithTextAndOriginator(string text, string originator)
    {
        _transactionText = text;
        _originator = originator;

        // Capture the saga context during ProcessAsync
        _mockSagaCoordinator
            .Setup(x => x.ProcessAsync(
                It.IsAny<StartTransaction>(),
                It.IsAny<SagaContext>()))
            .Callback<StartTransaction, ISagaContext>((msg, ctx) => _capturedContext = ctx)
            .Returns(Task.CompletedTask);

        _resultSagaId = await _sagaTransactionService.StartTransactionAsync(text, originator);
        _existingSagaId = _resultSagaId.Id;
    }

    [When(@"I start a new saga transaction")]
    public async Task WhenIStartANewSagaTransaction()
    {
        // Capture the saga context during ProcessAsync
        _mockSagaCoordinator
            .Setup(x => x.ProcessAsync(
                It.IsAny<StartTransaction>(),
                It.IsAny<SagaContext>()))
            .Callback<StartTransaction, ISagaContext>((msg, ctx) => _capturedContext = ctx)
            .Returns(Task.CompletedTask);

        _resultSagaId = await _sagaTransactionService.StartTransactionAsync(_transactionText, _originator);
    }

    [When(@"I complete the saga transaction with text ""(.*)"" and originator ""(.*)""")]
    public async Task WhenICompleteTheSagaTransactionWithTextAndOriginator(string text, string originator)
    {
        // Setup to trigger onCompleted callback
        _mockSagaCoordinator
            .Setup(x => x.ProcessAsync(
                It.IsAny<CompleteTransaction>(),
                It.IsAny<Func<CompleteTransaction, ISagaContext, Task>>(),
                It.IsAny<Func<CompleteTransaction, ISagaContext, Task>>(),
                It.IsAny<ISagaContext>()))
            .Callback<object, Func<CompleteTransaction, ISagaContext, Task>, Func<CompleteTransaction, ISagaContext, Task>, ISagaContext>(
                async (msg, onCompleted, onRejected, ctx) =>
                {
                    _capturedContext = ctx;
                    await onCompleted((CompleteTransaction)msg, ctx);
                })
            .Returns(Task.CompletedTask);

        _resultSagaId = await _sagaTransactionService.CompleteTransactionAsync(_existingSagaId, text, originator);
    }

    [When(@"I complete the saga transaction that results in rejection")]
    public async Task WhenICompleteTheSagaTransactionThatResultsInRejection()
    {
        // Setup to trigger onRejected callback
        _mockSagaCoordinator
            .Setup(x => x.ProcessAsync(
                It.IsAny<CompleteTransaction>(),
                It.IsAny<Func<CompleteTransaction, ISagaContext, Task>>(),
                It.IsAny<Func<CompleteTransaction, ISagaContext, Task>>(),
                It.IsAny<ISagaContext>()))
            .Callback<object, Func<CompleteTransaction, ISagaContext, Task>, Func<CompleteTransaction, ISagaContext, Task>, ISagaContext>(
                async (msg, onCompleted, onRejected, ctx) =>
                {
                    _capturedContext = ctx;
                    await onRejected((CompleteTransaction)msg, ctx);
                })
            .Returns(Task.CompletedTask);

        _resultSagaId = await _sagaTransactionService.CompleteTransactionAsync(_existingSagaId, "Rejection text", _originator);
    }

    [Then(@"a new saga should be created")]
    public void ThenANewSagaShouldBeCreated()
    {
        _mockSagaCoordinator.Verify(
            x => x.ProcessAsync(
                It.Is<StartTransaction>(st => st.Text == _transactionText),
                It.IsAny<SagaContext>()),
            Times.Once);
    }

    [Then(@"the saga should have a valid SagaId")]
    public void ThenTheSagaShouldHaveAValidSagaId()
    {
        Assert.NotNull(_resultSagaId);
        Assert.False(string.IsNullOrEmpty(_resultSagaId.Id));
    }

    [Then(@"the saga context should contain the originator ""(.*)""")]
    public void ThenTheSagaContextShouldContainTheOriginator(string expectedOriginator)
    {
        Assert.NotNull(_capturedContext);
        Assert.Equal(expectedOriginator, _capturedContext.Originator);
    }

    [Then(@"the saga should be marked as completed")]
    public void ThenTheSagaShouldBeMarkedAsCompleted()
    {
        _mockSagaCoordinator.Verify(
            x => x.ProcessAsync(
                It.IsAny<CompleteTransaction>(),
                It.IsAny<Func<CompleteTransaction, ISagaContext, Task>>(),
                It.IsAny<Func<CompleteTransaction, ISagaContext, Task>>(),
                It.IsAny<ISagaContext>()),
            Times.Once);
    }

    [Then(@"the completion should be logged")]
    public void ThenTheCompletionShouldBeLogged()
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("completed successfully")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Then(@"the saga should be marked as rejected")]
    public void ThenTheSagaShouldBeMarkedAsRejected()
    {
        _mockSagaCoordinator.Verify(
            x => x.ProcessAsync(
                It.IsAny<CompleteTransaction>(),
                It.IsAny<Func<CompleteTransaction, ISagaContext, Task>>(),
                It.IsAny<Func<CompleteTransaction, ISagaContext, Task>>(),
                It.IsAny<ISagaContext>()),
            Times.Once);
    }

    [Then(@"the rejection should be logged with error level")]
    public void ThenTheRejectionShouldBeLoggedWithErrorLevel()
    {
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("rejected")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}