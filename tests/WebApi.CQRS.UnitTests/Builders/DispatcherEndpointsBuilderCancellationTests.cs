using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Queries;
using Genocs.WebApi;
using Genocs.WebApi.CQRS.Builders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Genocs.WebApi.CQRS.UnitTests.Builders;

public class DispatcherEndpointsBuilderCancellationTests
{
    [Fact]
    public async Task Post_CommandDispatchUsesRequestAbortedToken()
    {
        var fakeBuilder = new CapturingEndpointsBuilder();
        var sut = new DispatcherEndpointsBuilder(fakeBuilder);

        CancellationToken? tokenSeenByDispatcher = null;

        var services = new ServiceCollection();
        services.AddSingleton<ICommandDispatcher>(new TestCommandDispatcher((_, token) => tokenSeenByDispatcher = token));

        using ServiceProvider provider = services.BuildServiceProvider();
        var context = new DefaultHttpContext
        {
            RequestServices = provider,
            Response = { Body = new MemoryStream() }
        };

        using var cts = new CancellationTokenSource();
        context.RequestAborted = cts.Token;

        sut.Post<TestCommand>("/commands");

        Assert.NotNull(fakeBuilder.PostHandler);
        await fakeBuilder.PostHandler!(new TestCommand(), context);

        Assert.True(tokenSeenByDispatcher.HasValue);
        Assert.Equal(context.RequestAborted, tokenSeenByDispatcher.Value);
    }

    [Fact]
    public async Task Get_QueryDispatchUsesRequestAbortedToken()
    {
        var fakeBuilder = new CapturingEndpointsBuilder();
        var sut = new DispatcherEndpointsBuilder(fakeBuilder);

        CancellationToken? tokenSeenByDispatcher = null;

        var services = new ServiceCollection();
        services.AddSingleton<IQueryDispatcher>(new TestQueryDispatcher((_, token) => tokenSeenByDispatcher = token));

        using ServiceProvider provider = services.BuildServiceProvider();
        var context = new DefaultHttpContext
        {
            RequestServices = provider,
            Response = { Body = new MemoryStream() }
        };

        using var cts = new CancellationTokenSource();
        context.RequestAborted = cts.Token;

        sut.Get<TestQuery, TestResult>("/queries");

        Assert.NotNull(fakeBuilder.GetQueryHandler);
        await fakeBuilder.GetQueryHandler!(new TestQuery(), context);

        Assert.True(tokenSeenByDispatcher.HasValue);
        Assert.Equal(context.RequestAborted, tokenSeenByDispatcher.Value);
    }

    [Fact]
    public async Task Get_QueryWithoutAfterDispatch_Returns404WhenResultIsNull()
    {
        var fakeBuilder = new CapturingEndpointsBuilder();
        var sut = new DispatcherEndpointsBuilder(fakeBuilder);

        var services = new ServiceCollection();
        services.AddSingleton<IQueryDispatcher>(new TestQueryDispatcher((_, _) => { }));

        using ServiceProvider provider = services.BuildServiceProvider();
        var context = new DefaultHttpContext
        {
            RequestServices = provider,
            Response = { Body = new MemoryStream() }
        };

        sut.Get<TestQuery, TestResult>("/queries");

        Assert.NotNull(fakeBuilder.GetQueryHandler);
        await fakeBuilder.GetQueryHandler!(new TestQuery(), context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    private sealed class TestCommand : ICommand
    {
    }

    private sealed class TestQuery : IQuery<TestResult>
    {
    }

    private sealed class TestResult
    {
    }

    private sealed class TestCommandDispatcher(Action<ICommand, CancellationToken> onSend) : ICommandDispatcher
    {
        private readonly Action<ICommand, CancellationToken> _onSend = onSend;

        public Task SendAsync<T>(T command, CancellationToken cancellationToken = default)
            where T : class, ICommand
        {
            _onSend(command, cancellationToken);
            return Task.CompletedTask;
        }

        public Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default)
            where TCommand : class, ICommand<TResult>
            => Task.FromResult(default(TResult)!);
    }

    private sealed class TestQueryDispatcher(Action<IQuery<TestResult>, CancellationToken> onQuery) : IQueryDispatcher
    {
        private readonly Action<IQuery<TestResult>, CancellationToken> _onQuery = onQuery;

        public Task<TResult?> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
            => Task.FromResult(default(TResult));

        public Task<TResult?> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default)
            where TQuery : class, IQuery<TResult>
        {
            if (query is IQuery<TestResult> typedQuery)
            {
                _onQuery(typedQuery, cancellationToken);
            }

            return Task.FromResult(default(TResult));
        }
    }

    private sealed class CapturingEndpointsBuilder : IEndpointsBuilder
    {
        public Func<TestCommand, HttpContext, Task>? PostHandler { get; private set; }
        public Func<TestQuery, HttpContext, Task>? GetQueryHandler { get; private set; }

        public IEndpointsBuilder Get(string path, Func<HttpContext, Task>? context = null, Action<IEndpointConventionBuilder>? endpoint = null, bool auth = false, string? roles = null, params string[] policies)
            => this;

        public IEndpointsBuilder Get<T>(string path, Func<T, HttpContext, Task>? context = null, Action<IEndpointConventionBuilder>? endpoint = null, bool auth = false, string? roles = null, params string[] policies)
            where T : class
            => this;

        public IEndpointsBuilder Get<TRequest, TResult>(string path, Func<TRequest, HttpContext, Task>? context = null, Action<IEndpointConventionBuilder>? endpoint = null, bool auth = false, string? roles = null, params string[] policies)
            where TRequest : class
        {
            if (typeof(TRequest) == typeof(TestQuery) && context is not null)
            {
                GetQueryHandler = (query, httpContext) => context((TRequest)(object)query, httpContext);
            }

            return this;
        }

        public IEndpointsBuilder Post(string path, Func<HttpContext, Task>? context = null, Action<IEndpointConventionBuilder>? endpoint = null, bool auth = false, string? roles = null, params string[] policies)
            => this;

        public IEndpointsBuilder Post<T>(string path, Func<T, HttpContext, Task>? context = null, Action<IEndpointConventionBuilder>? endpoint = null, bool auth = false, string? roles = null, params string[] policies)
            where T : class
        {
            if (typeof(T) == typeof(TestCommand) && context is not null)
            {
                PostHandler = (command, httpContext) => context((T)(object)command, httpContext);
            }

            return this;
        }

        public IEndpointsBuilder Put(string path, Func<HttpContext, Task>? context = null, Action<IEndpointConventionBuilder>? endpoint = null, bool auth = false, string? roles = null, params string[] policies)
            => this;

        public IEndpointsBuilder Put<T>(string path, Func<T, HttpContext?, Task>? context = null, Action<IEndpointConventionBuilder>? endpoint = null, bool auth = false, string? roles = null, params string[] policies)
            where T : class
            => this;

        public IEndpointsBuilder Delete(string path, Func<HttpContext?, Task>? context = null, Action<IEndpointConventionBuilder>? endpoint = null, bool auth = false, string? roles = null, params string[] policies)
            => this;

        public IEndpointsBuilder Delete<T>(string path, Func<T, HttpContext?, Task>? context = null, Action<IEndpointConventionBuilder>? endpoint = null, bool auth = false, string? roles = null, params string[] policies)
            where T : class
            => this;
    }
}
