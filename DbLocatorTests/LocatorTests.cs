using DbLocator;
using DbLocatorTests.Fixtures;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class LocatorTests(DbLocatorFixture fixture)
{
    private readonly DbLocatorFixture _fixture = fixture;

    [Fact]
    public void SqlConnection_IsInitializedWithCorrectConnectionString()
    {
        var connectionString = _fixture.ConnectionString;
        var locator = new Locator(connectionString);

        Assert.NotNull(locator.SqlConnection);
        Assert.Equal(connectionString, locator.SqlConnection.ConnectionString);
    }
}
