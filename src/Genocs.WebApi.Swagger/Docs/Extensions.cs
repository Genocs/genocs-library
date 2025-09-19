using System.Reflection;
using Genocs.Core.Builders;
using Genocs.WebApi.Swagger.Docs.Builders;
using Genocs.WebApi.Swagger.Docs.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Genocs.WebApi.Swagger.Docs;

public static class Extensions
{
    private const string SectionName = "swagger";
    private const string RegistryName = "docs.swagger";

    public static IGenocsBuilder AddSwaggerDocs(this IGenocsBuilder builder, string sectionName = SectionName)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = SectionName;
        }

        SwaggerOptions settings = builder.GetOptions<SwaggerOptions>(sectionName);

        if (settings is null)
        {
            return builder;
        }

        return builder.AddSwaggerDocs(settings);
    }

    public static IGenocsBuilder AddSwaggerDocs(this IGenocsBuilder builder, Func<ISwaggerOptionsBuilder, ISwaggerOptionsBuilder> buildOptions)
    {
        SwaggerOptions settings = buildOptions(new SwaggerOptionsBuilder()).Build();

        if (settings is null)
        {
            return builder;
        }

        return builder.AddSwaggerDocs(settings);
    }

    public static IGenocsBuilder AddSwaggerDocs(this IGenocsBuilder builder, SwaggerOptions settings)
    {
        if (!settings.Enabled || !builder.TryRegister(RegistryName))
        {
            return builder;
        }

        // TODO: Double-check if this is necessary
        builder.Services.AddSingleton(settings);

        // Register the Swagger generator, defining 1 or more Swagger documents
        builder.Services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();

            c.SwaggerDoc(
                            settings.Name,
                            new OpenApiInfo
                            {
                                Version = settings.Version,
                                Title = settings.Title,
                                Description = settings.Description,
                                TermsOfService = new Uri(settings.TermsOfService ?? "https://www.genocs.com/terms_and_conditions.html"),
                                Contact = new OpenApiContact
                                {
                                    Name = settings.ContactName,
                                    Email = settings.ContactEmail,
                                    Url = new Uri(settings.ContactUrl ?? "https://www.genocs.com")
                                },
                                License = new OpenApiLicense
                                {
                                    Name = settings.LicenseName,
                                    Url = new Uri(settings.LicenseUrl ?? "https://opensource.org/license/mit/")
                                }
                            });

            if (settings.IncludeSecurity)
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Name = "Bearer",
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            }

            // This is required to make the custom operation ids work
            // It's required to be used by LangChain tools
            c.CustomOperationIds(oid =>
            {
                if (oid.ActionDescriptor is not ControllerActionDescriptor actionDescriptor)
                {
                    return null; // default behavior
                }

                return oid.GroupName switch
                {
                    "v1" => $"{actionDescriptor.ActionName}",
                    _ => $"_{actionDescriptor.ActionName}", // default behavior
                };
            });

            // Add list of servers

            if (settings.Servers != null)
            {
                foreach (var server in settings.Servers)
                {
                    c.AddServer(new OpenApiServer() { Url = server.Url, Description = server.Description });
                }
            }

            // c.AddServer(new OpenApiServer() { Url = "http://localhost:5300", Description = "Local version to be used for development" });
            // c.AddServer(new OpenApiServer() { Url = "http://fiscanner-api", Description = "Containerized version to be used into with docker or k8s" });
            // c.AddServer(new OpenApiServer() { Url = "https://fiscanner-api.azurewebsites.net", Description = "Production deployed on Azure" });

            string documentationFile = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml");
            c.IncludeXmlComments(documentationFile);
        });

        return builder;
    }

    public static IApplicationBuilder UseSwaggerDocs(this IApplicationBuilder builder)
    {
        var options = builder.ApplicationServices.GetRequiredService<SwaggerOptions>();
        if (!options.Enabled)
        {
            return builder;
        }

        string routePrefix = string.IsNullOrWhiteSpace(options.RoutePrefix) ? string.Empty : options.RoutePrefix;

        builder.UseStaticFiles()
            .UseSwagger(c =>
            {
                c.RouteTemplate = string.Concat(routePrefix, "/{documentName}/swagger.json");
            });

        return options.ReDocEnabled
            ? builder.UseReDoc(c =>
            {
                c.RoutePrefix = routePrefix;
                c.SpecUrl = $"{options.Name}/swagger.json";
            })
            : builder.UseSwaggerUI(c =>
            {
                c.RoutePrefix = routePrefix;
                c.SwaggerEndpoint($"/{routePrefix}/{options.Name}/swagger.json".FormatEmptyRoutePrefix(),
                    options.Title);
            });
    }

    /// <summary>
    /// Replaces leading double forward slash caused by an empty route prefix.
    /// </summary>
    /// <param name="route"></param>
    /// <returns></returns>
    private static string FormatEmptyRoutePrefix(this string route)
    {
        return route.Replace("//", "/");
    }
}