using Genocs.Common.Cqrs.Commands;
using Genocs.Common.Cqrs.Events;
using Genocs.Core.Cqrs.Events;
using Genocs.WebApi.Helpers;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Genocs.WebApi.Cqrs.Middlewares;

public class PublicContractsMiddleware
{
    private const string ContentType = "application/json";
    private readonly RequestDelegate _next;
    private readonly string _endpoint;
    private readonly bool _attributeRequired;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        WriteIndented = true
    };

    private static readonly ContractTypes Contracts = new();
    private static int _initialized;
    private static string _serializedContracts = "{}";

    public PublicContractsMiddleware(RequestDelegate next, string endpoint, Type attributeType, bool attributeRequired)
    {
        _next = next;
        _endpoint = endpoint;
        _attributeRequired = attributeRequired;
        if (_initialized == 1)
        {
            return;
        }

        Load(attributeType);
    }

    public Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path != _endpoint)
        {
            return _next(context);
        }

        context.Response.ContentType = ContentType;
        context.Response.WriteAsync(_serializedContracts);

        return Task.CompletedTask;
    }

    private void Load(Type attributeType)
    {
        if (Interlocked.Exchange(ref _initialized, 1) == 1)
        {
            return;
        }

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var contracts = assemblies.SelectMany(a => a.GetTypes())
            .Where(t => (!_attributeRequired || t.GetCustomAttribute(attributeType) is not null) && !t.IsInterface)
            .ToArray();

        foreach (var command in contracts.Where(t => typeof(ICommand).IsAssignableFrom(t)))
        {
            object? instance = command.GetDefaultInstance();
            string? name = instance?.GetType().Name;

            if (!string.IsNullOrWhiteSpace(name) && instance != null)
            {
                if (Contracts.Commands.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Command: '{name}' already exists.");
                }

                Contracts.Commands[name] = instance;
            }

        }

        foreach (var @event in contracts.Where(t => typeof(IEvent).IsAssignableFrom(t) &&
                                                    t != typeof(RejectedEvent)))
        {
            object? instance = @event.GetDefaultInstance();
            string? name = instance?.GetType().Name;

            if (!string.IsNullOrWhiteSpace(name) && instance != null)
            {
                if (Contracts.Events.ContainsKey(name))
                {
                    throw new InvalidOperationException($"Event: '{name}' already exists.");
                }

                Contracts.Events[name] = instance;
            }
        }

        _serializedContracts = JsonSerializer.Serialize(Contracts, SerializerOptions);
    }

    private class ContractTypes
    {
        public Dictionary<string, object> Commands { get; } = new();
        public Dictionary<string, object> Events { get; } = new();
    }
}