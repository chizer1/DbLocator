using DbLocator.Utilities;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class EncryptionTests
{
    private const string EncryptionKey = "TestEncryptionKey123";

    [Fact]
    public void Encrypt_ShouldReturnPlainText_WhenEncryptionKeyIsNullOrEmpty()
    {
        var encryption = new Encryption(null);
        string plainText = "TestPlainText";

        string result = encryption.Encrypt(plainText);

        Assert.Equal(plainText, result);
    }

    [Fact]
    public void Encrypt_ShouldReturnEncryptedText_WhenValidEncryptionKey()
    {
        var encryption = new Encryption(EncryptionKey);
        string plainText = "TestPlainText";

        string result = encryption.Encrypt(plainText);

        Assert.NotNull(result);
        Assert.NotEqual(plainText, result);
    }

    [Fact]
    public void Decrypt_ShouldReturnEncryptedText_WhenEncryptionKeyIsNullOrEmpty()
    {
        var encryption = new Encryption(null);
        string encryptedText = "TestEncryptedText";

        string result = encryption.Decrypt(encryptedText);

        Assert.Equal(encryptedText, result);
    }

    [Fact]
    public void Decrypt_ShouldReturnOriginalPlainText_WhenValidEncryptionKey()
    {
        var encryption = new Encryption(EncryptionKey);
        string plainText = "TestPlainText";
        string encryptedText = encryption.Encrypt(plainText);

        string result = encryption.Decrypt(encryptedText);

        Assert.Equal(plainText, result);
    }

    [Fact]
    public void Encrypt_ShouldThrowArgumentNullException_WhenPlainTextIsNull()
    {
        var encryption = new Encryption(EncryptionKey);

        Assert.Throws<ArgumentNullException>(() => encryption.Encrypt(null));
    }

    [Fact]
    public void Decrypt_ShouldThrowFormatException_WhenEncryptedTextIsInvalidBase64()
    {
        var encryption = new Encryption(EncryptionKey);
        string invalidEncryptedText = "InvalidBase64";

        Assert.Throws<FormatException>(() => encryption.Decrypt(invalidEncryptedText));
    }
}
