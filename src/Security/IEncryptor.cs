namespace Genocs.Security;

/// <summary>
/// Defines methods for encrypting and decrypting data using a specified key. Supports both string and byte array
/// operations to accommodate various data formats.
/// </summary>
/// <remarks>Implementations of this interface should ensure that cryptographic operations are performed securely
/// and efficiently. The interface allows for flexible encryption scenarios, including the use of initialization vectors
/// for byte array operations. Callers are responsible for providing valid keys and, where applicable, initialization
/// vectors that meet the requirements of the underlying encryption algorithm.</remarks>
public interface IEncryptor
{
    /// <summary>
    /// Encrypts the specified plaintext data using the provided encryption key.
    /// </summary>
    /// <remarks>Keep the encryption key secure, as it is required for decrypting the data. The method may
    /// throw exceptions if the input data or key is invalid.</remarks>
    /// <param name="data">The plaintext data to encrypt. This parameter must not be null or empty.</param>
    /// <param name="key">The encryption key to use for securing the data. The key must meet the requirements of the underlying encryption
    /// algorithm.</param>
    /// <returns>A string containing the encrypted representation of the input data.</returns>
    string Encrypt(string data, string key);

    /// <summary>
    /// Decrypts the specified encrypted data using the provided key.
    /// </summary>
    /// <remarks>Ensure that the key provided is the same as the one used for encryption to successfully
    /// retrieve the original data.</remarks>
    /// <param name="data">The encrypted data to decrypt. This parameter must not be null or empty.</param>
    /// <param name="key">The key used to decrypt the data. This parameter must match the key used during encryption and cannot be null.</param>
    /// <returns>The decrypted string representation of the original data. Returns null if decryption fails.</returns>
    string Decrypt(string data, string key);

    /// <summary>
    /// Encrypts the specified data using the provided initialization vector and encryption key.
    /// </summary>
    /// <remarks>Ensure that both the key and initialization vector are securely generated and managed. The
    /// encryption algorithm and its requirements may vary depending on the implementation. Using different keys or
    /// initialization vectors will produce different encrypted outputs for the same input data.</remarks>
    /// <param name="data">The data to encrypt. This parameter must not be null or empty.</param>
    /// <param name="iv">The initialization vector to use for encryption. The length must match the requirements of the encryption
    /// algorithm.</param>
    /// <param name="key">The encryption key to use. The length must match the requirements of the encryption algorithm.</param>
    /// <returns>A byte array containing the encrypted representation of the input data.</returns>
    byte[] Encrypt(byte[] data, byte[] iv, byte[] key);

    /// <summary>
    /// Decrypts the specified encrypted data using the provided initialization vector and key.
    /// </summary>
    /// <remarks>The key and initialization vector must match those used during encryption to successfully
    /// decrypt the data.</remarks>
    /// <param name="data">The encrypted data to decrypt. This parameter must not be null.</param>
    /// <param name="iv">The initialization vector to use for decryption. This parameter must not be null and must be the correct length
    /// for the encryption algorithm.</param>
    /// <param name="key">The key to use for decryption. This parameter must not be null and must be the correct length for the encryption
    /// algorithm.</param>
    /// <returns>A byte array containing the decrypted data. Returns an empty array if decryption fails.</returns>
    byte[] Decrypt(byte[] data, byte[] iv, byte[] key);
}