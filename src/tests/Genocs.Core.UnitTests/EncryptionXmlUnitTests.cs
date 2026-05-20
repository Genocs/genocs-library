using System.Security.Cryptography;
using Genocs.Core.Extensions;
using Xunit;

namespace Genocs.Core.UnitTests;

public class EncryptionXmlUnitTests
{
    [Fact]
    public void FromXmlFile_ThrowsArgumentNullException_WhenRsaIsNull()
    {
        ArgumentNullException ex = Assert.Throws<ArgumentNullException>(() => Encryption.FromXmlFile(null!, "key.xml"));

        Assert.Equal("rsa", ex.ParamName);
    }

    [Fact]
    public void FromXmlFile_ThrowsArgumentException_WhenPathIsEmpty()
    {
        using RSA rsa = RSA.Create();

        Assert.Throws<ArgumentException>(() => rsa.FromXmlFile(string.Empty));
    }

    [Fact]
    public void FromXmlFile_ThrowsFormatException_WhenXmlIsMalformed()
    {
        using RSA rsa = RSA.Create();
        string filePath = WriteTempXml("<RSAKeyValue><Modulus>");

        try
        {
            Assert.Throws<FormatException>(() => rsa.FromXmlFile(filePath));
        }
        finally
        {
            TryDelete(filePath);
        }
    }

    [Fact]
    public void FromXmlFile_ThrowsFormatException_WhenRootIsNotRsaKeyValue()
    {
        using RSA rsa = RSA.Create();
        string filePath = WriteTempXml("<NotRsaKeyValue />");

        try
        {
            Assert.Throws<FormatException>(() => rsa.FromXmlFile(filePath));
        }
        finally
        {
            TryDelete(filePath);
        }
    }

    [Fact]
    public void FromXmlFile_ThrowsFormatException_WhenRequiredNodeIsMissing()
    {
        using RSA rsa = RSA.Create();
        string modulus = Convert.ToBase64String([1, 2, 3]);
        string filePath = WriteTempXml($"<RSAKeyValue><Modulus>{modulus}</Modulus></RSAKeyValue>");

        try
        {
            Assert.Throws<FormatException>(() => rsa.FromXmlFile(filePath));
        }
        finally
        {
            TryDelete(filePath);
        }
    }

    [Fact]
    public void FromXmlFile_ThrowsFormatException_WhenNodeContainsInvalidBase64()
    {
        using RSA rsa = RSA.Create();
        string filePath = WriteTempXml("<RSAKeyValue><Modulus>AA==</Modulus><Exponent>@@@</Exponent></RSAKeyValue>");

        try
        {
            Assert.Throws<FormatException>(() => rsa.FromXmlFile(filePath));
        }
        finally
        {
            TryDelete(filePath);
        }
    }

    [Fact]
    public void FromXmlFile_ThrowsFormatException_WhenPrivateKeyIsIncomplete()
    {
        using RSA rsa = RSA.Create();
        using RSA source = RSA.Create(2048);
        RSAParameters pub = source.ExportParameters(false);

        string modulus = Convert.ToBase64String(pub.Modulus!);
        string exponent = Convert.ToBase64String(pub.Exponent!);
        string filePath = WriteTempXml($"<RSAKeyValue><Modulus>{modulus}</Modulus><Exponent>{exponent}</Exponent><P>AA==</P></RSAKeyValue>");

        try
        {
            Assert.Throws<FormatException>(() => rsa.FromXmlFile(filePath));
        }
        finally
        {
            TryDelete(filePath);
        }
    }

    [Fact]
    public void FromXmlFile_ImportsValidPublicKey()
    {
        using RSA source = RSA.Create(2048);
        using RSA target = RSA.Create();
        string filePath = Path.Combine(Path.GetTempPath(), $"genocs-rsa-{Guid.NewGuid():N}.xml");

        try
        {
            source.ToXmlFile(false, filePath);
            target.FromXmlFile(filePath);

            RSAParameters expected = source.ExportParameters(false);
            RSAParameters actual = target.ExportParameters(false);

            Assert.Equal(expected.Modulus, actual.Modulus);
            Assert.Equal(expected.Exponent, actual.Exponent);
        }
        finally
        {
            TryDelete(filePath);
        }
    }

    private static string WriteTempXml(string xml)
    {
        string filePath = Path.Combine(Path.GetTempPath(), $"genocs-rsa-{Guid.NewGuid():N}.xml");
        File.WriteAllText(filePath, xml);
        return filePath;
    }

    private static void TryDelete(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
