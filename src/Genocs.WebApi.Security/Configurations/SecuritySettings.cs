namespace Genocs.WebApi.Security.Configurations;

public class SecuritySettings
{
    public CertificateSettings? Certificate { get; set; }

    public class CertificateSettings
    {
        public bool Enabled { get; set; }
        public string? Header { get; set; }
        public bool AllowSubdomains { get; set; }
        public IEnumerable<string>? AllowedDomains { get; set; }
        public IEnumerable<string>? AllowedHosts { get; set; }
        public IDictionary<string, AclSettings>? Acl { get; set; }
        public bool SkipRevocationCheck { get; set; }

        public string GetHeaderName() => string.IsNullOrWhiteSpace(Header) ? "Certificate" : Header;

        public class AclSettings
        {
            public string? ValidIssuer { get; set; }
            public string? ValidThumbprint { get; set; }
            public string? ValidSerialNumber { get; set; }
            public IEnumerable<string>? Permissions { get; set; }
        }
    }
}