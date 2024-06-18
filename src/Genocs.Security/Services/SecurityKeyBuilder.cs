using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Xml;

namespace Genocs.Security.Services;

public static class SecurityKeyBuilder
{
    /// <summary>
    /// Create a new instance of <see cref="SecurityKey"/> using the provided secret.
    /// </summary>
    /// <param name="secret">The secret key as xml string.</param>
    /// <returns>The created RSA.</returns>
    public static SecurityKey CreateRsaSecurityKey(string secret)
    {
        RSA rsa = RSA.Create();
        rsa = FromCustomXmlString(rsa, secret);
        return new RsaSecurityKey(rsa);
    }

    /// <summary>
    /// Helper method to create a RSA instance from a custom XML string.
    /// </summary>
    /// <param name="rsa">The RSA object instance.</param>
    /// <param name="xmlKey">The secret key as xml string.</param>
    /// <returns>The created RSA.</returns>
    /// <exception cref="Exception">In case the secret key is invalid xml.</exception>
    private static RSA FromCustomXmlString(RSA rsa, string xmlKey)
    {
        RSAParameters parameters = default(RSAParameters);
        XmlDocument xmlDocument = new XmlDocument();
        xmlDocument.LoadXml(xmlKey);
        if (xmlDocument.DocumentElement != null && xmlDocument.DocumentElement!.Name.Equals("RSAKeyValue"))
        {
            foreach (XmlNode childNode in xmlDocument.DocumentElement!.ChildNodes)
            {
                switch (childNode.Name)
                {
                    case "Modulus":
                        parameters.Modulus = string.IsNullOrEmpty(childNode.InnerText) ? null : Convert.FromBase64String(childNode.InnerText);
                        break;
                    case "Exponent":
                        parameters.Exponent = string.IsNullOrEmpty(childNode.InnerText) ? null : Convert.FromBase64String(childNode.InnerText);
                        break;
                    case "P":
                        parameters.P = string.IsNullOrEmpty(childNode.InnerText) ? null : Convert.FromBase64String(childNode.InnerText);
                        break;
                    case "Q":
                        parameters.Q = string.IsNullOrEmpty(childNode.InnerText) ? null : Convert.FromBase64String(childNode.InnerText);
                        break;
                    case "DP":
                        parameters.DP = string.IsNullOrEmpty(childNode.InnerText) ? null : Convert.FromBase64String(childNode.InnerText);
                        break;
                    case "DQ":
                        parameters.DQ = string.IsNullOrEmpty(childNode.InnerText) ? null : Convert.FromBase64String(childNode.InnerText);
                        break;
                    case "InverseQ":
                        parameters.InverseQ = string.IsNullOrEmpty(childNode.InnerText) ? null : Convert.FromBase64String(childNode.InnerText);
                        break;
                    case "D":
                        parameters.D = string.IsNullOrEmpty(childNode.InnerText) ? null : Convert.FromBase64String(childNode.InnerText);
                        break;
                }
            }

            rsa.ImportParameters(parameters);
            return rsa;
        }

        throw new Exception("Invalid XML RSA key.");
    }
}