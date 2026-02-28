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

    internal static void RegisterConventions()
    {
        BsonSerializer.RegisterSerializer(typeof(decimal), new DecimalSerializer(BsonType.Decimal128));

        // Move to standard GuidRepresentation
        BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.CSharpLegacy));

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
}
