using System.Security.Cryptography.X509Certificates;

namespace Genocs.Secrets.HashicorpKeyVault;

public interface ICertificatesIssuer
{
    Task<X509Certificate2> IssueAsync();
}