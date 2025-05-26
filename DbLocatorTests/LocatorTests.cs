using DbLocator;
using DbLocatorTests.Fixtures;
using Microsoft.Data.SqlClient;
using Xunit;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class LocatorTests(DbLocatorFixture fixture)
{
    private readonly DbLocatorFixture _fixture = fixture;

    [Fact]
    public void SqlConnection_IsInitializedWithCorrectConnectionString()
    {
        // Arrange & Act
        var connectionString = _fixture.ConnectionString;
        var locator = new Locator(connectionString);

        // Assert
        Assert.NotNull(locator.SqlConnection);
        Assert.Equal(connectionString, locator.SqlConnection.ConnectionString);
    }
}
