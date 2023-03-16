using Genocs.Docs.Swagger;
using Genocs.Core.Builders;
using Genocs.WebApi.Swagger.Docs;
using Genocs.WebApi.Swagger.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Genocs.WebApi.Swagger;

public static class Extensions
{
    private const string SectionName = "swagger";

    public static IGenocsBuilder AddWebApiSwaggerDocs(this IGenocsBuilder builder, string sectionName = SectionName)
    {
        if (string.IsNullOrWhiteSpace(sectionName))
        {
            sectionName = SectionName;
        }

        return builder.AddWebApiSwaggerDocs(b => b.AddSwaggerDocs(sectionName));
    }

    public static IGenocsBuilder AddWebApiSwaggerDocs(this IGenocsBuilder builder,
        Func<ISwaggerOptionsBuilder, ISwaggerOptionsBuilder> buildOptions)
        => builder.AddWebApiSwaggerDocs(b => b.AddSwaggerDocs(buildOptions));

    public static IGenocsBuilder AddWebApiSwaggerDocs(this IGenocsBuilder builder, SwaggerOptions options)
        => builder.AddWebApiSwaggerDocs(b => b.AddSwaggerDocs(options));

    private static IGenocsBuilder AddWebApiSwaggerDocs(this IGenocsBuilder builder, Action<IGenocsBuilder> registerSwagger)
    {
        registerSwagger(builder);
        builder.Services.AddSwaggerGen(c => c.DocumentFilter<WebApiDocumentFilter>());
        return builder;
    }
}