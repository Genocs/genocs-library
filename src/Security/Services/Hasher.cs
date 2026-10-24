using System.Security.Cryptography;
using System.Text;

namespace Genocs.Security.Services;

internal sealed class Hasher : IHasher
{
    public string Hash(string data)
    {
        byte[] hash = Hash(Encoding.UTF8.GetBytes(data));
        var builder = new StringBuilder();
        foreach (byte @byte in hash)
        {
            builder.Append(@byte.ToString("x2"));
        }

        return builder.ToString();
    }

    public byte[] Hash(byte[] data)
    {
        if (data is null || !data.Any())
        {
            throw new ArgumentException("Data to be hashed cannot be empty.", nameof(data));
        }

        using var sha256Hash = SHA256.Create();

        return sha256Hash.ComputeHash(data);
    }

    public byte[] Hash(byte[] data, byte[] key)
    {
        if (data is null || !data.Any())
        {
            throw new ArgumentException("Data to be hashed cannot be empty.", nameof(data));
        }

        if (key is null || !key.Any())
        {
            throw new ArgumentException("Key to be hashed cannot be empty.", nameof(key));
        }

        HMACSHA256 sha256Hash = new HMACSHA256(key);
        return sha256Hash.ComputeHash(data);
    }

    public string Hash(string data, string key)
    {
        byte[] bytesData = Encoding.Default.GetBytes(data);
        byte[] bytesKey = Encoding.Default.GetBytes(key);

        byte[] hash = Hash(bytesData, bytesKey);

        // convert the byte array to a hexadecimal string
        var builder = new StringBuilder();
        foreach (byte @byte in hash)
        {
            builder.Append(@byte.ToString("x2"));
        }

        return builder.ToString();

    }
}