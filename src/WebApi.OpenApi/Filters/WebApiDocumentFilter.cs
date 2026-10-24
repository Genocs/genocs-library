#if NET10_0_OR_GREATER
using Microsoft.OpenApi;
#else
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
#endif

using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace Genocs.WebApi.OpenApi.Filters;

internal sealed class WebApiDocumentFilter(WebApiEndpointDefinitions definitions) : IDocumentFilter
{
    private const string InBody = "body";
    private const string InQuery = "query";

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

        foreach (var pathDefinition in definitions.GroupBy(d => d.Path))
        {
            var pathItem = new OpenApiPathItem();

            foreach (var methodDefinition in pathDefinition)
            {
                if (!TryCreateOperation(pathItem, methodDefinition.Method, out OpenApiOperation operation))
                {
                    continue;
                }

                operation.Responses = [];
                operation.Parameters = [];

                foreach (var parameter in methodDefinition.Parameters)
                {
                    if (parameter.In is InBody)
                    {
                        operation.RequestBody = new OpenApiRequestBody()
                        {
                            Content = new Dictionary<string, OpenApiMediaType>()
                            {
                                {
                                    "application/json", new OpenApiMediaType()
                                    {
                                        Schema = GetSchema(parameter, context, jsonSerializerOptions)
                                    }
                                }
                            }
                        };
                    }
                    else if (parameter.In is InQuery)
                    {
                        // Policy: query-bound contracts are represented as query parameters, never request bodies.
                        operation.Parameters.Add(new OpenApiParameter
                        {
                            Name = string.IsNullOrWhiteSpace(parameter.Name) ? "query" : parameter.Name,
                            In = ParameterLocation.Query,
                            Schema = GetSchema(parameter, context, jsonSerializerOptions)
                        });
                    }
                }

                foreach (var response in methodDefinition.Responses)
                {
                    operation.Responses.Add(response.StatusCode.ToString(), new OpenApiResponse
                    {
                        Content = new Dictionary<string, OpenApiMediaType>
                        {
                            {
                                "application/json", new OpenApiMediaType
                                {
                                    Schema = GetSchema(response, context, jsonSerializerOptions)
                                }
                            }
                        }
                    });
                }
            }

            swaggerDoc.Paths.Add($"/{pathDefinition.Key}", pathItem);
        }
    }

    private static bool TryCreateOperation(OpenApiPathItem pathItem, string method, out OpenApiOperation operation)
    {
        operation = new OpenApiOperation();

        switch (method.ToUpperInvariant())
        {
#if NET10_0_OR_GREATER
            case "GET":
                pathItem.AddOperation(HttpMethod.Get, operation);
                return true;
            case "POST":
                pathItem.AddOperation(HttpMethod.Post, operation);
                return true;
            case "PUT":
                pathItem.AddOperation(HttpMethod.Put, operation);
                return true;
            case "DELETE":
                pathItem.AddOperation(HttpMethod.Delete, operation);
                return true;
#else
            case "GET":
                pathItem.AddOperation(OperationType.Get, operation);
                return true;
            case "POST":
                pathItem.AddOperation(OperationType.Post, operation);
                return true;
            case "PUT":
                pathItem.AddOperation(OperationType.Put, operation);
                return true;
            case "DELETE":
                pathItem.AddOperation(OperationType.Delete, operation);
                return true;
#endif
            default:
                operation = new OpenApiOperation();
                return false;
        }
    }

#if NET10_0_OR_GREATER
    private static IOpenApiSchema GetSchema(WebApiEndpointParameter parameter, DocumentFilterContext context, JsonSerializerOptions _)
    {
        return GetSchema(parameter.Type, context);
    }

    private static IOpenApiSchema GetSchema(WebApiEndpointResponse response, DocumentFilterContext context, JsonSerializerOptions _)
    {
        return GetSchema(response.Type, context);
    }

    private static IOpenApiSchema GetSchema(Type? type, DocumentFilterContext context)
    {
        if (type is null)
        {
            return new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
            };
        }

        return context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository);
    }
#else
    private static OpenApiSchema GetSchema(WebApiEndpointParameter parameter, DocumentFilterContext context, JsonSerializerOptions jsonSerializerOptions)
    {
        OpenApiSchema schema = GetSchema(parameter.Type, context);
        schema.Example = new OpenApiString(JsonSerializer.Serialize(parameter.Example, jsonSerializerOptions));
        return schema;
    }

    private static OpenApiSchema GetSchema(WebApiEndpointResponse response, DocumentFilterContext context, JsonSerializerOptions jsonSerializerOptions)
    {
        OpenApiSchema schema = GetSchema(response.Type, context);
        schema.Example = new OpenApiString(JsonSerializer.Serialize(response.Example, jsonSerializerOptions));
        return schema;
    }

    private static OpenApiSchema GetSchema(Type? type, DocumentFilterContext context)
    {
        if (type is null)
        {
            return new OpenApiSchema
            {
                Type = "object",
            };
        }

        return context.SchemaGenerator.GenerateSchema(type, context.SchemaRepository);
    }
#endif
}