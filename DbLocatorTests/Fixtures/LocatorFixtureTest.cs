using DbLocator;

namespace DbLocatorTests.Fixtures;

public class LocatorFixtureTest
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenConnectionStringIsNull()
    {
        string nullConnectionString = null;

        Assert.Throws<ArgumentException>(() => new Locator(nullConnectionString, null, null));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenConnectionStringIsEmpty()
    {
        string emptyConnectionString = string.Empty;

        Assert.Throws<ArgumentException>(() => new Locator(emptyConnectionString, null, null));
    }

    [Fact]
    public void Constructor_ShouldThrowArgumentException_WhenConnectionStringIsWhitespace()
    {
        string whitespaceConnectionString = "   ";

        Assert.Throws<ArgumentException>(() => new Locator(whitespaceConnectionString, null, null));
    }
}
