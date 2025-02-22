using System.Security.Cryptography;
using System.Text;

namespace DbLocator.Utilities;

internal class Encryption(string encryptionKey)
{
    public string Encrypt(string plainText)
    {
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
        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
        byte[] decryptedBytes = decryptor.TransformFinalBlock(
            encryptedBytes,
            0,
            encryptedBytes.Length
        );
        return Encoding.UTF8.GetString(decryptedBytes);
    }
}
