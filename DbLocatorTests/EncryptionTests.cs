using DbLocator.Utilities;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class EncryptionTests
{
    private const string EncryptionKey = "TestEncryptionKey123";

    [Fact]
    public void Encrypt_ShouldReturnPlainText_WhenEncryptionKeyIsNullOrEmpty()
    {
        // Arrange
        var encryption = new Encryption(null);
        string plainText = "TestPlainText";

        // Act
        string result = encryption.Encrypt(plainText);

        // Assert
        Assert.Equal(plainText, result);
    }

    [Fact]
    public void Encrypt_ShouldReturnEncryptedText_WhenValidEncryptionKey()
    {
        // Arrange
        var encryption = new Encryption(EncryptionKey);
        string plainText = "TestPlainText";

        // Act
        string result = encryption.Encrypt(plainText);

        // Assert
        Assert.NotNull(result);
        Assert.NotEqual(plainText, result);
    }

    [Fact]
    public void Decrypt_ShouldReturnEncryptedText_WhenEncryptionKeyIsNullOrEmpty()
    {
        // Arrange
        var encryption = new Encryption(null);
        string encryptedText = "TestEncryptedText";

        // Act
        string result = encryption.Decrypt(encryptedText);

        // Assert
        Assert.Equal(encryptedText, result);
    }

    [Fact]
    public void Decrypt_ShouldReturnOriginalPlainText_WhenValidEncryptionKey()
    {
        // Arrange
        var encryption = new Encryption(EncryptionKey);
        string plainText = "TestPlainText";
        string encryptedText = encryption.Encrypt(plainText);

        // Act
        string result = encryption.Decrypt(encryptedText);

        // Assert
        Assert.Equal(plainText, result);
    }

    [Fact]
    public void Encrypt_ShouldThrowArgumentNullException_WhenPlainTextIsNull()
    {
        // Arrange
        var encryption = new Encryption(EncryptionKey);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => encryption.Encrypt(null));
    }

    [Fact]
    public void Decrypt_ShouldThrowFormatException_WhenEncryptedTextIsInvalidBase64()
    {
        // Arrange
        var encryption = new Encryption(EncryptionKey);
        string invalidEncryptedText = "InvalidBase64";

        // Act & Assert
        Assert.Throws<FormatException>(() => encryption.Decrypt(invalidEncryptedText));
    }
}
