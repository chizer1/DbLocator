using DbLocator;

namespace DbLocatorTests.Fixtures;

public class LocatorFixtureTest
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConnectionStringIsNull()
    {
        // Arrange
        string nullConnectionString = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Locator(nullConnectionString, null, null));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenConnectionStringIsEmpty()
    {
        // Arrange
        string emptyConnectionString = string.Empty;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Locator(emptyConnectionString, null, null));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenConnectionStringIsWhitespace()
    {
        // Arrange
        string whitespaceConnectionString = "   ";

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Locator(whitespaceConnectionString, null, null));
    }
}
