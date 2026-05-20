namespace Genocs.Persistence.MongoDB.Configurations;

/// <summary>
/// Controls how GUID values are serialized to BSON.
/// </summary>
public enum MongoGuidRepresentationMode
{
    /// <summary>
    /// Uses GuidRepresentation.Standard for new deployments.
    /// </summary>
    Standard = 0,

    /// <summary>
    /// Uses GuidRepresentation.CSharpLegacy for compatibility with legacy datasets.
    /// </summary>
    CSharpLegacy = 1,
}