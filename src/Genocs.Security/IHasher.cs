namespace Genocs.Security;

/// <summary>
/// This provides interfaces to the <see cref="Services.Hasher"/> class.
/// </summary>
public interface IHasher
{
    /// <summary>
    /// Generates the hash value of the given data.
    /// </summary>
    /// <param name="data">The data used to create the hash.</param>
    /// <returns>The hash result as string.</returns>
    string Hash(string data);

    /// <summary>
    /// Generates the hash value of the given data.
    /// </summary>
    /// <param name="data">The data used to create the hash.</param>
    /// <param name="key">The private key used to used to create the hash.</param>
    /// <returns>The hash result as string.</returns>
    string Hash(string data, string key);

    /// <summary>
    /// Generates the hash value of the given data.
    /// </summary>
    /// <param name="data">The data used to create the hash as byte array.</param>
    /// <returns>The hash result as byte array.</returns>
    byte[] Hash(byte[] data);

    /// <summary>
    /// Generates the hash value of the given data.
    /// </summary>
    /// <param name="data">The data used to create the hash.</param>
    /// <param name="key">The private key used to used to create the hash.</param>
    /// <returns>The hash result as byte array.</returns>
    byte[] Hash(byte[] data, byte[] key);
}