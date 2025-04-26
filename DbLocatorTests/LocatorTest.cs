using DbLocator;

namespace DbLocatorTests
{
    public class LocatorTests
    {
        [Fact]
        public void Constructor_ShouldThrowArgumentException_WhenConnectionStringIsNull()
        {
            var exception = Assert.Throws<ArgumentException>(() => new Locator(null));
            Assert.Equal(
                "DbLocator connection string is required. (Parameter 'dbLocatorConnectionString')",
                exception.Message
            );
        }
    }
}
