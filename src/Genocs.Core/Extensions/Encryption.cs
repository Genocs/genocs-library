using System.Security.Cryptography;
using System.Xml;

namespace Genocs.Core.Extensions;

// https://github.com/dotnet/corefx/issues/23686

public static class Encryption
{
    public static void FromXmlFile(this RSA rsa, string xmlFilePath)
    {
        ArgumentNullException.ThrowIfNull(rsa);
        if (string.IsNullOrWhiteSpace(xmlFilePath))
        {
            throw new ArgumentException("XML key file path must be provided.", nameof(xmlFilePath));
        }

        if (!File.Exists(xmlFilePath))
        {
            throw new FileNotFoundException("XML key file was not found.", xmlFilePath);
        }

        XmlDocument xmlDoc = LoadXmlDocument(xmlFilePath);
        XmlElement root = GetRsaRoot(xmlDoc);

        RSAParameters parameters = new RSAParameters
        {
            Modulus = ReadRequiredNode(root, "Modulus"),
            Exponent = ReadRequiredNode(root, "Exponent")
        };

        if (HasPrivateParameters(root))
        {
            parameters.P = ReadRequiredNode(root, "P");
            parameters.Q = ReadRequiredNode(root, "Q");
            parameters.DP = ReadRequiredNode(root, "DP");
            parameters.DQ = ReadRequiredNode(root, "DQ");
            parameters.InverseQ = ReadRequiredNode(root, "InverseQ");
            parameters.D = ReadRequiredNode(root, "D");
        }

        try
        {
            rsa.ImportParameters(parameters);
        }
        catch (CryptographicException ex)
        {
            throw new CryptographicException("RSA parameters could not be imported from XML.", ex);
        }
    }

    public static void ToXmlFile(this RSA rsa, bool includePrivateParameters, string xmlFilePath)
    {
        ArgumentNullException.ThrowIfNull(rsa);
        if (string.IsNullOrWhiteSpace(xmlFilePath))
        {
            throw new ArgumentException("XML key file path must be provided.", nameof(xmlFilePath));
        }

        RSAParameters parameters = rsa.ExportParameters(includePrivateParameters);

        File.WriteAllText(
            xmlFilePath,
            string.Format(
                "<RSAKeyValue><Modulus>{0}</Modulus><Exponent>{1}</Exponent><P>{2}</P><Q>{3}</Q><DP>{4}</DP><DQ>{5}</DQ><InverseQ>{6}</InverseQ><D>{7}</D></RSAKeyValue>",
                parameters.Modulus != null ? Convert.ToBase64String(parameters.Modulus) : null,
                parameters.Exponent != null ? Convert.ToBase64String(parameters.Exponent) : null,
                parameters.P != null ? Convert.ToBase64String(parameters.P) : null,
                parameters.Q != null ? Convert.ToBase64String(parameters.Q) : null,
                parameters.DP != null ? Convert.ToBase64String(parameters.DP) : null,
                parameters.DQ != null ? Convert.ToBase64String(parameters.DQ) : null,
                parameters.InverseQ != null ? Convert.ToBase64String(parameters.InverseQ) : null,
                parameters.D != null ? Convert.ToBase64String(parameters.D) : null));
    }

    private static XmlDocument LoadXmlDocument(string xmlFilePath)
    {
        string xmlContent = File.ReadAllText(xmlFilePath);
        XmlDocument xmlDoc = new XmlDocument();

        try
        {
            xmlDoc.LoadXml(xmlContent);
            return xmlDoc;
        }
        catch (XmlException ex)
        {
            throw new FormatException("RSA key XML is malformed.", ex);
        }
    }

    private static XmlElement GetRsaRoot(XmlDocument xmlDoc)
    {
        XmlElement? root = xmlDoc.DocumentElement;
        if (root is null || !root.Name.Equals("RSAKeyValue", StringComparison.Ordinal))
        {
            throw new FormatException("RSA key XML must contain RSAKeyValue as the document root.");
        }

        return root;
    }

    private static bool HasPrivateParameters(XmlElement root)
    {
        string[] privateNodes = ["P", "Q", "DP", "DQ", "InverseQ", "D"];
        return privateNodes.Any(nodeName => !string.IsNullOrWhiteSpace(root[nodeName]?.InnerText));
    }

    private static byte[] ReadRequiredNode(XmlElement root, string nodeName)
    {
        XmlNode? node = root[nodeName];
        if (node is null)
        {
            throw new FormatException($"RSA key XML is missing required node '{nodeName}'.");
        }

        if (string.IsNullOrWhiteSpace(node.InnerText))
        {
            throw new FormatException($"RSA key XML node '{nodeName}' cannot be empty.");
        }

        try
        {
            return Convert.FromBase64String(node.InnerText);
        }
        catch (FormatException ex)
        {
            throw new FormatException($"RSA key XML node '{nodeName}' is not valid Base64.", ex);
        }
    }
}
