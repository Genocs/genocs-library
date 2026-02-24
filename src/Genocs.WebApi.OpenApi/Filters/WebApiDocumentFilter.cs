#if NET10_0_OR_GREATER

using Microsoft.AspNetCore.Components;
using Microsoft.OpenApi;

#else

using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;

#endif

using Swashbuckle.AspNetCore.SwaggerGen;
using System.Data.Common;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Genocs.WebApi.OpenApi.Filters;

internal sealed class WebApiDocumentFilter(WebApiEndpointDefinitions definitions) : IDocumentFilter
{
    private const string InBody = "body";
    private const string InQuery = "query";

    private readonly Func<OpenApiPathItem, string, OpenApiOperation?> _getOperation = static (item, path) =>
    {
#if NET10_0_OR_GREATER

        switch (path)
        {
            case "GET":
                item.AddOperation(HttpMethod.Get, new OpenApiOperation());
                return item.Operations[HttpMethod.Get];
            case "POST":
                item.AddOperation(HttpMethod.Post, new OpenApiOperation());
                return item.Operations[HttpMethod.Post];
            case "PUT":
                item.AddOperation(HttpMethod.Put, new OpenApiOperation());
                return item.Operations[HttpMethod.Put];
            case "DELETE":
                item.AddOperation(HttpMethod.Delete, new OpenApiOperation());
                return item.Operations[HttpMethod.Delete];
        }

#else
        switch (path)
        {
            case "GET":
                item.AddOperation(OperationType.Get, new OpenApiOperation());
                return item.Operations[OperationType.Get];
            case "POST":
                item.AddOperation(OperationType.Post, new OpenApiOperation());
                return item.Operations[OperationType.Post];
            case "PUT":
                item.AddOperation(OperationType.Put, new OpenApiOperation());
                return item.Operations[OperationType.Put];
            case "DELETE":
                item.AddOperation(OperationType.Delete, new OpenApiOperation());
                return item.Operations[OperationType.Delete];
        }
#endif

        return null;
    };

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

        foreach (var pathDefinition in definitions.GroupBy(d => d.Path))
        {
            var pathItem = new OpenApiPathItem();

            foreach (var methodDefinition in pathDefinition)
            {
                var operation = _getOperation(pathItem, methodDefinition.Method);
                operation.Responses = new OpenApiResponses();
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
                                        Schema = GetSchema(parameter, jsonSerializerOptions)
                                    }
                                }
                            }
                        };
                    }
                    else if (parameter.In is InQuery)
                    {
                        if (parameter.Type.GetInterface("IQuery") is not null)
                        {
                            operation.RequestBody = new OpenApiRequestBody()
                            {
                                Content = new Dictionary<string, OpenApiMediaType>()
                                {
                                    {
                                        "application/json", new OpenApiMediaType
                                        {
                                            Schema = GetSchema(parameter, jsonSerializerOptions)
                                        }
                                    }
                                }
                            };
                        }
                        else
                        {
                            operation.Parameters.Add(new OpenApiParameter
                            {
                                Name = parameter.Name,
                                Schema = GetSchema(parameter, jsonSerializerOptions)
                            });
                        }
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
                                    Schema = GetSchema(response, jsonSerializerOptions)
                                }
                            }
                        }
                    });
                }
            }

            swaggerDoc.Paths.Add($"/{pathDefinition.Key}", pathItem);
        }
    }

#if NET10_0_OR_GREATER
    private IOpenApiSchema GetSchema(WebApiEndpointParameter parameter, JsonSerializerOptions options)
    {
        return new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            //Example = new JsonNode(JsonSerializer.Serialize(parameter.Example, options)
        };
    }

    private IOpenApiSchema GetSchema(WebApiEndpointResponse response, JsonSerializerOptions options)
    {
        return new OpenApiSchema
        {
            Type = JsonSchemaType.String,
            //Example = new JsonNode(JsonSerializer.Serialize(parameter.Example, options)
        };
    }

#else

    private OpenApiSchema GetSchema(WebApiEndpointParameter parameter, JsonSerializerOptions jsonSerializerOptions)
    {
        return new OpenApiSchema
        {
            Type = parameter.Type?.Name,
            Example = new OpenApiString(JsonSerializer.Serialize(parameter.Example, jsonSerializerOptions))
        };
    }

    private OpenApiSchema GetSchema(WebApiEndpointResponse response, JsonSerializerOptions jsonSerializerOptions)
    {
        return new OpenApiSchema
        {
            Type = response.Type?.Name,
            Example = new OpenApiString(JsonSerializer.Serialize(response.Example, jsonSerializerOptions))
        };
    }

#endif
}