using Genocs.Persistence.MongoDB.Configurations;
using Genocs.Persistence.MongoDB.Extensions;
using MongoDB.Bson;
using Xunit;

namespace Genocs.Persistence.MongoDB.UnitTests;

public class GuidRepresentationCompatibilityTests
{
    [Fact]
    public void MongoOptions_DefaultGuidRepresentationMode_IsStandard()
    {
        var options = new MongoOptions();

        Assert.Equal(MongoGuidRepresentationMode.Standard, options.GuidRepresentationMode);
    }

    [Theory]
    [InlineData(MongoGuidRepresentationMode.Standard, GuidRepresentation.Standard)]
    [InlineData(MongoGuidRepresentationMode.CSharpLegacy, GuidRepresentation.CSharpLegacy)]
    public void ToGuidRepresentation_MapsConfiguredMode(MongoGuidRepresentationMode mode, GuidRepresentation expected)
    {
        var resolved = ServiceCollectionExtensions.ToGuidRepresentation(mode);

        Assert.Equal(expected, resolved);
    }
}
