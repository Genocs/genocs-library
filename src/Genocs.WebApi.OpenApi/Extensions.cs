//using Genocs.Core.Builders;
//using Genocs.WebApi.OpenApi.Docs;
//using Genocs.WebApi.OpenApi.Docs.Configurations;
//using Genocs.WebApi.OpenApi.Filters;
//using Microsoft.Extensions.DependencyInjection;

//namespace Genocs.WebApi.OpenApi;

//public static class Extensions
//{
//    private const string SectionName = "openapi";

//    public static IGenocsBuilder AddOpenApiDocs(this IGenocsBuilder builder, string sectionName = SectionName)
//    {
//        if (string.IsNullOrWhiteSpace(sectionName))
//        {
//            sectionName = SectionName;
//        }

//        return builder.AddOpenApiDocs(b => b.AddOpenApiDocs(sectionName));
//    }

//    public static IGenocsBuilder AddOpenApiDocs(this IGenocsBuilder builder, Func<IOpenApiOptionsBuilder, IOpenApiOptionsBuilder> buildOptions)
//        => builder.AddOpenApiDocs(b => b.AddOpenApiDocs(buildOptions));

//    public static IGenocsBuilder AddOpenApiDocs(this IGenocsBuilder builder, OpenApiOptions options)
//        => builder.AddOpenApiDocs(b => b.AddOpenApiDocs(options));

//    private static IGenocsBuilder AddOpenApiDocs(this IGenocsBuilder builder, Action<IGenocsBuilder> register)
//    {
//        register(builder);
//        builder.Services.AddEndpointsApiExplorer();
//        builder.Services.AddSwaggerGen(c => c.DocumentFilter<WebApiDocumentFilter>());
//        return builder;
//    }
//}