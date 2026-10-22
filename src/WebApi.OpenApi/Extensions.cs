using System.Reflection;
using Genocs.Core.Builders;
using Genocs.WebApi.OpenApi.Builders;
using Genocs.WebApi.OpenApi.Configurations;
using Genocs.WebApi.OpenApi.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Swagger;

#if NET10_0_OR_GREATER
using Microsoft.OpenApi;
#else
using Microsoft.OpenApi.Models;
#endif

namespace Genocs.WebApi.OpenApi;

public static class Extensions
{
    private const string RegistryName = "docs.openapi";

    public static IGenocsBuilder AddOpenApiDocs(this IGenocsBuilder builder, string sectionName = OpenApiOptions.Position)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = OpenApiOptions.Position;
        }

        OpenApiOptions settings = builder.GetOptions<OpenApiOptions>(sectionName);

        if (settings is null)
        {
            return builder;
        }

        return builder.AddOpenApiDocs(settings);
    }

    public static IGenocsBuilder AddOpenApiDocs(this IGenocsBuilder builder, Func<IOpenApiOptionsBuilder, IOpenApiOptionsBuilder> buildOptions)
    {
        OpenApiOptions settings = buildOptions(new OpenApiOptionsBuilder()).Build();

        if (settings is null)
        {
            return builder;
        }

        return builder.AddOpenApiDocs(settings);
    }

    /// <summary>
    /// Use this method to add and configure Swagger documentation.
    /// Remove in case you are not using AddOpenApiDocs.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="settings">The Settings.</param>
    /// <returns>The Genocs builder to be used for chain.</returns>
    public static IGenocsBuilder AddOpenApiDocs(this IGenocsBuilder builder, OpenApiOptions settings)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(settings);

        // Keep options available in DI even when docs are disabled so UseOpenApiDocs can no-op safely.
        builder.Services.AddSingleton(settings);

        if (!settings.Enabled || !builder.TryRegister(RegistryName))
        {
            return builder;
        }

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();
            c.DocumentFilter<WebApiDocumentFilter>();

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

            // Add security definition if needed
            if (settings.IncludeSecurity)
            {
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });
#if NET10_0_OR_GREATER
                c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecuritySchemeReference("Bearer"),
                        []
                    }
                });
#else
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        []
                    }
                });
#endif
            }

            // Add list of servers
            if (settings.Servers != null)
            {
                foreach (var server in settings.Servers)
                {
                    c.AddServer(new OpenApiServer() { Url = server.Url, Description = server.Description });
                }
            }

            string documentationFile = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml");
            c.IncludeXmlComments(documentationFile);
        });

        /*

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

        });

        */

        return builder;
    }

    public static IApplicationBuilder UseOpenApiDocs(this IApplicationBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        OpenApiOptions? options = builder.ApplicationServices.GetService<OpenApiOptions>();
        if (options is null || !options.Enabled)
        {
            return builder;
        }

        // OpenApi docs were not registered on this host, so there is nothing to wire at runtime.
        if (builder.ApplicationServices.GetService<ISwaggerProvider>() is null)
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
                c.SwaggerEndpoint($"/{routePrefix}/{options.Name}/swagger.json".FormatEmptyRoutePrefix(), options.Title);
            });
    }

    /// <summary>
    /// Replaces leading double forward slash caused by an empty route prefix.
    /// </summary>
    /// <param name="route">The string to parse.</param>
    /// <returns></returns>
    private static string FormatEmptyRoutePrefix(this string route)
    {
        return route.Replace("//", "/");
    }
}