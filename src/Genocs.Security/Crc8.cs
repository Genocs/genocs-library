using System.Text;

namespace Genocs.Security;

/// <summary>
///  CRC 8-bit checksum calculator.
/// </summary>
public static class Crc8
{
    // x8 + x7 + x6 + x4 + x2 + 1
    private const byte Poly = 0xd5;

    private static readonly byte[] Table = new byte[256];

    /// <summary>
    /// Compute the checksum of the byte array.
    /// </summary>
    /// <param name="bytes"></param>
    /// <returns></returns>
    public static byte ComputeChecksum(params byte[] bytes)
    {
        byte crc = 0;
        if (bytes != null && bytes.Length > 0)
        {
            foreach (byte b in bytes)
            {
                crc = Table[crc ^ b];
            }
        }

        return crc;
    }

    /// <summary>
    /// Compute the checksum of the string.
    /// </summary>
    /// <param name="payload">The string where to calculate the CRC.</param>
    /// <returns>The Checksum.</returns>
    /// <exception cref="ArgumentNullException">If the payload is a null, empty or whitespaces string.</exception>
    public static byte ComputeChecksum(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new ArgumentNullException(nameof(payload));
        }

        byte[] buffer = Encoding.UTF8.GetBytes(payload);
        return ComputeChecksum(buffer);
    }

    static Crc8()
    {
        for (int i = 0; i < 256; ++i)
        {
            int temp = i;
            for (int j = 0; j < 8; ++j)
            {
                if ((temp & 0x80) != 0)
                {
                    temp = (temp << 1) ^ Poly;
                }
                else
                {
                    temp <<= 1;
                }
            }

            Table[i] = (byte)temp;
        }
    }
}
