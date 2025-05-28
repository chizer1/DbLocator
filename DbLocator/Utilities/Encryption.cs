using System.Security.Cryptography;
using System.Text;

namespace DbLocator.Utilities;

internal class Encryption(string encryptionKey)
{
    /// <summary>
    /// Encrypts the specified plain text using AES encryption with the provided encryption key.
    /// </summary>
    /// <param name="plainText">The plain text to encrypt.</param>
    /// <returns>The encrypted text as a Base64-encoded string, or the original plain text if the encryption key is null or empty.</returns>
    public string Encrypt(string plainText)
    {
        if (plainText == null)
            throw new ArgumentNullException(nameof(plainText), "Plain text is required");
        if (string.IsNullOrEmpty(encryptionKey))
            return plainText;

        byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32));
        byte[] ivBytes = new byte[16];

        using Aes aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = ivBytes;

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        byte[] encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
        return Convert.ToBase64String(encryptedBytes);
    }

    /// <summary>
    /// Decrypts the specified encrypted text using AES decryption with the provided encryption key.
    /// </summary>
    /// <param name="encryptedText">The encrypted text as a Base64-encoded string.</param>
    /// <returns>The decrypted plain text, or the original encrypted text if the encryption key is null or empty.</returns>
    public string Decrypt(string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptionKey))
            return encryptedText;

        byte[] keyBytes = Encoding.UTF8.GetBytes(encryptionKey.PadRight(32));
        byte[] ivBytes = new byte[16];

        using Aes aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = ivBytes;

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        byte[] encryptedBytes;
        try
        {
            encryptedBytes = Convert.FromBase64String(encryptedText);
        }
        catch (FormatException)
        {
            throw new FormatException("Invalid Base64 string");
        }
        byte[] decryptedBytes = decryptor.TransformFinalBlock(
            encryptedBytes,
            0,
            encryptedBytes.Length
        );
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
