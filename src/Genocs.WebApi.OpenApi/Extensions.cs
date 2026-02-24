using Genocs.Core.Builders;
using Genocs.WebApi.OpenApi.Docs;
using Genocs.WebApi.OpenApi.Docs.Configurations;
using Genocs.WebApi.OpenApi.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.WebApi.OpenApi;

public static class Extensions
{
    private const string SectionName = "openapi";

    public static IGenocsBuilder AddWebApiSwaggerDocs(this IGenocsBuilder builder, string sectionName = SectionName)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = SectionName;
        }

        return builder.AddWebApiSwaggerDocs(b => b.AddSwaggerDocs(sectionName));
    }

    public static IGenocsBuilder AddWebApiSwaggerDocs(this IGenocsBuilder builder, Func<ISwaggerOptionsBuilder, ISwaggerOptionsBuilder> buildOptions)
        => builder.AddWebApiSwaggerDocs(b => b.AddSwaggerDocs(buildOptions));

    public static IGenocsBuilder AddWebApiSwaggerDocs(this IGenocsBuilder builder, SwaggerOptions options)
        => builder.AddWebApiSwaggerDocs(b => b.AddSwaggerDocs(options));

    private static IGenocsBuilder AddWebApiSwaggerDocs(this IGenocsBuilder builder, Action<IGenocsBuilder> registerSwagger)
    {
        registerSwagger(builder);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c => c.DocumentFilter<WebApiDocumentFilter>());
        return builder;
    }
}