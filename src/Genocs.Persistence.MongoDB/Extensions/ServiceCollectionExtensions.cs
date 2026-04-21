using Genocs.Persistence.MongoDB.Configurations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Bson.Serialization.Serializers;

namespace Genocs.Persistence.MongoDB.Extensions;

/// <summary>
/// Service Collection Extension for MongoDB Repository setup.
/// </summary>
public static class ServiceCollectionExtensions
{

    internal static void RegisterConventions(MongoGuidRepresentationMode guidRepresentationMode = MongoGuidRepresentationMode.Standard)
    {
        BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));

        BsonSerializer.RegisterSerializer(new GuidSerializer(ToGuidRepresentation(guidRepresentationMode)));

        BsonSerializer.RegisterSerializer(
                                            typeof(decimal?),
                                            new NullableSerializer<decimal>(new DecimalSerializer(BsonType.Decimal128)));

        ConventionRegistry.Register("genocs", new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new IgnoreExtraElementsConvention(true),
            new EnumRepresentationConvention(BsonType.String),
        }, _ => true);
    }

    internal static GuidRepresentation ToGuidRepresentation(MongoGuidRepresentationMode mode)
    {
        return mode switch
        {
            MongoGuidRepresentationMode.CSharpLegacy => GuidRepresentation.CSharpLegacy,
            _ => GuidRepresentation.Standard,
        };
    }
}
