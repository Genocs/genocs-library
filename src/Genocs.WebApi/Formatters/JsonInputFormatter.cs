using Microsoft.AspNetCore.Mvc.Formatters;
using Open.Serialization.Json;
using System.Collections.Concurrent;
using System.Reflection;

namespace Genocs.WebApi.Formatters;

internal class JsonInputFormatter : IInputFormatter
{
    private const string EmptyJson = "{}";
    private readonly ConcurrentDictionary<Type, MethodInfo> _methods = new();
    private readonly IJsonSerializer _serializer;
    private readonly MethodInfo _deserializeMethod;

    public JsonInputFormatter(IJsonSerializer serializer)
    {
        _serializer = serializer;
        _deserializeMethod = _serializer.GetType().GetMethods()
            .Single(m => m.IsGenericMethod && m.Name == nameof(_serializer.Deserialize));
    }

    public bool CanRead(InputFormatterContext context)
    {
        return true;
    }

    public async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
    {
        if (!_methods.TryGetValue(context.ModelType, out var method))
        {
            method = _deserializeMethod.MakeGenericMethod(context.ModelType);
            _methods.TryAdd(context.ModelType, method);
        }

        var request = context.HttpContext.Request;
        string json = string.Empty;
        if (request.Body is not null)
        {
            using var streamReader = new StreamReader(request.Body);
            json = await streamReader.ReadToEndAsync();
        }

        if (string.IsNullOrWhiteSpace(json))
        {
            json = EmptyJson;
        }

        object? result = method.Invoke(_serializer, [json]);

        return await InputFormatterResult.SuccessAsync(result);
    }
}