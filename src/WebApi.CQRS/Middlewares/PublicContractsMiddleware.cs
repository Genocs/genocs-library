using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Genocs.Common.CQRS.Commands;
using Genocs.Common.CQRS.Events;
using Genocs.WebApi.Helpers;
using Microsoft.AspNetCore.Http;

namespace Genocs.WebApi.CQRS.Middlewares;

public class PublicContractsMiddleware
{
    private const string ContentType = "application/json";
    private readonly RequestDelegate _next;
    private readonly string _endpoint;
    private readonly string _serializedContracts;
    private readonly Func<IEnumerable<Type>>? _contractTypesFactory;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
        WriteIndented = true
    };

    public PublicContractsMiddleware(RequestDelegate next, string endpoint, Type attributeType, bool attributeRequired, Func<IEnumerable<Type>>? contractTypesFactory = null)
    {
        _next = next;
        _endpoint = endpoint;
        _contractTypesFactory = contractTypesFactory;
        _serializedContracts = Load(attributeType, attributeRequired, _contractTypesFactory);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path != _endpoint)
        {
            await _next(context);
            return;
        }

        context.Response.ContentType = ContentType;
        await context.Response.WriteAsync(_serializedContracts);
    }

    private static string Load(Type attributeType, bool attributeRequired, Func<IEnumerable<Type>>? contractTypesFactory)
    {
        var contractTypes = new ContractTypes();

        var contracts = ResolveContractTypes(contractTypesFactory)
            .Where(t => (!attributeRequired || t.GetCustomAttribute(attributeType) is not null) && !t.IsInterface)
            .ToArray();

        foreach (var command in contracts.Where(t => typeof(ICommand).IsAssignableFrom(t)))
        {
            object? instance = command.GetDefaultInstance();
            string? name = ResolveContractName(command, contractTypes.Commands);

            if (!string.IsNullOrWhiteSpace(name) && instance != null)
            {
                contractTypes.Commands[name] = instance;
            }

        }

        foreach (var @event in contracts.Where(t => typeof(IEvent).IsAssignableFrom(t) &&
                                                    t != typeof(RejectedEvent)))
        {
            object? instance = @event.GetDefaultInstance();
            string? name = ResolveContractName(@event, contractTypes.Events);

            if (!string.IsNullOrWhiteSpace(name) && instance != null)
            {
                contractTypes.Events[name] = instance;
            }
        }

        return JsonSerializer.Serialize(contractTypes, SerializerOptions);
    }

    private static IEnumerable<Type> ResolveContractTypes(Func<IEnumerable<Type>>? contractTypesFactory)
    {
        if (contractTypesFactory is not null)
        {
            return ResolveLoadableTypes(contractTypesFactory);
        }

        return AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly => ResolveLoadableTypes(() => assembly.GetTypes()));
    }

    private static IEnumerable<Type> ResolveLoadableTypes(Func<IEnumerable<Type>> typeFactory)
    {
        try
        {
            return typeFactory().Where(type => type is not null);
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type is not null).Cast<Type>();
        }
        catch
        {
            return [];
        }
    }

    private static string ResolveContractName(Type contractType, IReadOnlyDictionary<string, object> existingContracts)
    {
        string simpleName = contractType.Name;
        if (!existingContracts.ContainsKey(simpleName))
        {
            return simpleName;
        }

        string? fullName = contractType.FullName;
        if (!string.IsNullOrWhiteSpace(fullName) && !existingContracts.ContainsKey(fullName))
        {
            return fullName;
        }

        string? assemblyName = contractType.Assembly.GetName().Name;
        return $"{assemblyName}:{fullName ?? simpleName}";
    }

    private class ContractTypes
    {
        public Dictionary<string, object> Commands { get; } = [];
        public Dictionary<string, object> Events { get; } = [];
    }
}