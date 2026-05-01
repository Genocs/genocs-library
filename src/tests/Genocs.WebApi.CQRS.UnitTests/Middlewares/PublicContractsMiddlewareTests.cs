using Genocs.WebApi.CQRS.Middlewares;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Genocs.WebApi.CQRS.UnitTests.Middlewares;

public class PublicContractsMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_AwaitsContractsWriteBeforeCompleting()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var writeStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var stream = new BlockingWriteStream(gate.Task, writeStarted);
        bool nextCalled = false;

        var middleware = new PublicContractsMiddleware(
            _ =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            },
            "/_contracts",
            typeof(TestContractAttribute),
            attributeRequired: true);

        var context = new DefaultHttpContext();
        context.Request.Path = "/_contracts";
        context.Response.Body = stream;

        Task invokeTask = middleware.InvokeAsync(context);

        await writeStarted.Task;

        Assert.False(invokeTask.IsCompleted);

        gate.SetResult();
        await invokeTask;

        Assert.False(nextCalled);
        Assert.Equal("application/json", context.Response.ContentType);
        Assert.NotEmpty(stream.GetWrittenText());
    }

    [Fact]
    public async Task InvokeAsync_WritesContractsPayload_WhenPathMatchesEndpoint()
    {
        var middleware = new PublicContractsMiddleware(
            _ => Task.CompletedTask,
            "/_contracts",
            typeof(TestContractAttribute),
            attributeRequired: true);

        var context = new DefaultHttpContext();
        context.Request.Path = "/_contracts";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        string payload = await reader.ReadToEndAsync();

        Assert.Equal("application/json", context.Response.ContentType);
        Assert.Contains("commands", payload, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("events", payload, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task InvokeAsync_UsesInstanceScopedContractSnapshot_WhenAttributeTypeDiffersAcrossInstances()
    {
        string payloadA = await InvokeForPayloadAsync(typeof(ContractGroupAAttribute), "/_contracts-a");
        string payloadB = await InvokeForPayloadAsync(typeof(ContractGroupBAttribute), "/_contracts-b");

        Assert.Contains(nameof(ContractGroupACommand), payloadA, StringComparison.Ordinal);
        Assert.Contains(nameof(ContractGroupAEvent), payloadA, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(ContractGroupBCommand), payloadA, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(ContractGroupBEvent), payloadA, StringComparison.Ordinal);

        Assert.Contains(nameof(ContractGroupBCommand), payloadB, StringComparison.Ordinal);
        Assert.Contains(nameof(ContractGroupBEvent), payloadB, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(ContractGroupACommand), payloadB, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(ContractGroupAEvent), payloadB, StringComparison.Ordinal);
    }

    private static async Task<string> InvokeForPayloadAsync(Type attributeType, string endpoint)
    {
        var middleware = new PublicContractsMiddleware(
            _ => Task.CompletedTask,
            endpoint,
            attributeType,
            attributeRequired: true);

        var context = new DefaultHttpContext();
        context.Request.Path = endpoint;
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync();
    }

    [Fact]
    public async Task InvokeAsync_UsesDeterministicKeys_WhenDuplicateContractNamesExist()
    {
        string payload = await InvokeForPayloadAsync(typeof(SharedContractAttribute), "/_contracts-duplicate");

        using var document = JsonDocument.Parse(payload);
        JsonElement commands = document.RootElement.GetProperty("commands");

        Assert.True(commands.TryGetProperty(nameof(DuplicateContractsA.DuplicateNameCommand), out _));
        Assert.Contains(commands.EnumerateObject(), p => p.Name.Contains(typeof(DuplicateContractsB.DuplicateNameCommand).FullName!, StringComparison.Ordinal));
    }

    [Fact]
    public async Task InvokeAsync_HandlesReflectionTypeLoadException_AndUsesLoadableTypes()
    {
        static IEnumerable<Type> ThrowingFactory() => throw new ReflectionTypeLoadException(
            [typeof(ContractGroupACommand), null!, typeof(ContractGroupAEvent)],
            [new TypeLoadException("boom")]);

        var middleware = new PublicContractsMiddleware(
            _ => Task.CompletedTask,
            "/_contracts-reflection",
            typeof(ContractGroupAAttribute),
            attributeRequired: true,
            contractTypesFactory: ThrowingFactory);

        var context = new DefaultHttpContext();
        context.Request.Path = "/_contracts-reflection";
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(context.Response.Body);
        string payload = await reader.ReadToEndAsync();

        Assert.Contains(nameof(ContractGroupACommand), payload, StringComparison.Ordinal);
        Assert.Contains(nameof(ContractGroupAEvent), payload, StringComparison.Ordinal);
    }

    [AttributeUsage(AttributeTargets.Class)]
    private sealed class TestContractAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    private sealed class ContractGroupAAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    private sealed class ContractGroupBAttribute : Attribute
    {
    }

    [ContractGroupA]
    private sealed class ContractGroupACommand : Genocs.Common.CQRS.Commands.ICommand
    {
    }

    [ContractGroupA]
    private sealed class ContractGroupAEvent : Genocs.Common.CQRS.Events.IEvent
    {
    }

    [ContractGroupB]
    private sealed class ContractGroupBCommand : Genocs.Common.CQRS.Commands.ICommand
    {
    }

    [ContractGroupB]
    private sealed class ContractGroupBEvent : Genocs.Common.CQRS.Events.IEvent
    {
    }

    private static class DuplicateContractsA
    {
        [SharedContract]
        internal sealed class DuplicateNameCommand : Genocs.Common.CQRS.Commands.ICommand
        {
        }
    }

    private static class DuplicateContractsB
    {
        [SharedContract]
        internal sealed class DuplicateNameCommand : Genocs.Common.CQRS.Commands.ICommand
        {
        }
    }

    private sealed class BlockingWriteStream(Task gate, TaskCompletionSource writeStarted) : MemoryStream
    {
        private readonly Task _gate = gate;
        private readonly TaskCompletionSource _writeStarted = writeStarted;

        public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            _ = _writeStarted.TrySetResult();
            await _gate.WaitAsync(cancellationToken);
            await base.WriteAsync(buffer, cancellationToken);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            _ = _writeStarted.TrySetResult();
            await _gate.WaitAsync(cancellationToken);
            await base.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public string GetWrittenText()
        {
            Position = 0;
            return Encoding.UTF8.GetString(ToArray());
        }
    }
}

[AttributeUsage(AttributeTargets.Class)]
internal sealed class SharedContractAttribute : Attribute
{
}
