namespace Genocs.WebApi.Security.Configurations;

public class SecurityOptions
{
    public CertificateOptions? Certificate { get; set; }

    public class CertificateOptions
    {
        /// <summary>
        /// Default section name.
        /// </summary>
        public const string Position = "certificate";

        /// <summary>
        /// It defines whether the section is enabled or not.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// The header name for the certificate.
        /// </summary>
        public string? Header { get; set; }

        /// <summary>
        /// It defines whether subdomains are allowed.
        /// </summary>
        public bool AllowSubdomains { get; set; }

        /// <summary>
        /// The list of allowed domains.
        /// </summary>
        public IEnumerable<string>? AllowedDomains { get; set; }

        /// <summary>
        /// The list of allowed hosts.
        /// </summary>
        public IEnumerable<string>? AllowedHosts { get; set; }

        /// <summary>
        /// The access control list (ACL) for the certificate.
        /// </summary>
        public IDictionary<string, AclOptions>? Acl { get; set; }

        /// <summary>
        /// It defines whether to skip the revocation check.
        /// </summary>
        public bool SkipRevocationCheck { get; set; }

        /// <summary>
        /// Gets the header name for the certificate.
        /// </summary>
        /// <returns>The header name.</returns>
        public string GetHeaderName()
            => string.IsNullOrWhiteSpace(Header) ? "Certificate" : Header;

        /// <summary>
        /// Represents the access control list (ACL) options for a certificate.
        /// </summary>
        public class AclOptions
        {
            /// <summary>
            /// The valid issuer for the certificate.
            /// </summary>
            public string? ValidIssuer { get; set; }

            /// <summary>
            /// The valid thumbprint for the certificate.
            /// </summary>
            public string? ValidThumbprint { get; set; }

            /// <summary>
            /// The valid serial number for the certificate.
            /// </summary>
            public string? ValidSerialNumber { get; set; }

            /// <summary>
            /// The list of permissions for the certificate.
            /// </summary>
            public IEnumerable<string>? Permissions { get; set; }
        }
    }
}