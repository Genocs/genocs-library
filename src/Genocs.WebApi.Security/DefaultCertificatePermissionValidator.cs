using Microsoft.AspNetCore.Http;
using System.Security.Cryptography.X509Certificates;

namespace Genocs.WebApi.Security;

internal sealed class DefaultCertificatePermissionValidator : ICertificatePermissionValidator
{
    public bool HasAccess(X509Certificate2 certificate, IEnumerable<string> permissions, HttpContext context)
        => true;
}