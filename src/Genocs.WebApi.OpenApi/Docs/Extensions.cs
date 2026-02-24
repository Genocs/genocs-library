using System.Reflection;
using Genocs.Core.Builders;
using Genocs.WebApi.OpenApi.Docs.Builders;
using Genocs.WebApi.OpenApi.Docs.Configurations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

#if NET10_0_OR_GREATER

#else

using Microsoft.OpenApi.Models;
#endif

namespace Genocs.WebApi.OpenApi.Docs;

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

    /// <summary>
    /// Use this method to add and configure Swagger documentation.
    /// Remove in case you are not using AddWebApiSwaggerDocs.
    /// </summary>
    /// <param name="builder">The Genocs builder.</param>
    /// <param name="settings">The Settings.</param>
    /// <returns>The Genocs builder to be used for chain.</returns>
    public static IGenocsBuilder AddSwaggerDocs(this IGenocsBuilder builder, SwaggerOptions settings)
    {
        if (!settings.Enabled || !builder.TryRegister(RegistryName))
        {
            return builder;
        }

        // TODO: Double-check if this is necessary
        builder.Services.AddSingleton(settings);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();

#if NET10_0_OR_GREATER

            c.SwaggerDoc(
                        settings.Name,
                        new Microsoft.OpenApi.OpenApiInfo
                        {
                            Version = settings.Version,
                            Title = settings.Title,
                            Description = settings.Description,
                            TermsOfService = new Uri(settings.TermsOfService ?? "https://www.genocs.com/terms_and_conditions.html"),
                            Contact = new Microsoft.OpenApi.OpenApiContact
                            {
                                Name = settings.ContactName,
                                Email = settings.ContactEmail,
                                Url = new Uri(settings.ContactUrl ?? "https://www.genocs.com")
                            },
                            License = new Microsoft.OpenApi.OpenApiLicense
                            {
                                Name = settings.LicenseName,
                                Url = new Uri(settings.LicenseUrl ?? "https://opensource.org/license/mit/")
                            }
                        });

#else

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
#endif

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
#if NET10_0_OR_GREATER

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                /*
                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Name = "Bearer",
                            Description = "bla bla bla",
                            Type = SecuritySchemeType.OAuth2,
                            BearerFormat = "JWT",
                            Scheme = "oauth2",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
                */

#else
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
#endif
            }

            // Add list of servers
            if (settings.Servers != null)
            {
                foreach (var server in settings.Servers)
                {

#if NET10_0_OR_GREATER
                    c.AddServer(new Microsoft.OpenApi.OpenApiServer() { Url = server.Url, Description = server.Description });

#else
                    c.AddServer(new Microsoft.OpenApi.Models.OpenApiServer() { Url = server.Url, Description = server.Description });
#endif
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
    /// <param name="route">The string to parse.</param>
    /// <returns></returns>
    private static string FormatEmptyRoutePrefix(this string route)
    {
        return route.Replace("//", "/");
    }
}