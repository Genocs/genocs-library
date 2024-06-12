using System.Security.Cryptography.X509Certificates;

namespace Genocs.Secrets.Vault;

/// <summary>
/// The Certification Service interface definition.
/// </summary>
public interface ICertificatesService
{
    IReadOnlyDictionary<string, X509Certificate2> All { get; }
    X509Certificate2? Get(string name);
    void Set(string name, X509Certificate2 certificate);
}