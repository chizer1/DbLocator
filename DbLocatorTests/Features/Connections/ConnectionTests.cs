using System.ComponentModel.DataAnnotations;
using DbLocator;
using DbLocator.Domain;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;

namespace DbLocatorTests.Features.Connections;

[Collection("DbLocator")]
public class ConnectionTests : IAsyncLifetime
{
    private readonly Locator _dbLocator;
    private readonly DbLocatorCache _cache;
    private readonly int _databaseServerID;
    private readonly byte _databaseTypeId;
    private readonly int _databaseId;
    private readonly string _databaseName;

    public ConnectionTests(DbLocatorFixture dbLocatorFixture)
    {
        _dbLocator = dbLocatorFixture.DbLocator;
        _databaseServerID = dbLocatorFixture.LocalhostServerId;
        _cache = dbLocatorFixture.LocatorCache;
        _databaseTypeId = _dbLocator.AddDatabaseType(TestHelpers.GetRandomString()).Result;
        _databaseName = TestHelpers.GetRandomString();
        _databaseId = _dbLocator
            .AddDatabase(_databaseName, _databaseServerID, _databaseTypeId, Status.Active)
            .Result;
    }

    public async Task InitializeAsync()
    {
        await _cache.Remove("databaseUsers");
        await _cache.Remove("databaseUserRoles");
    }

    public async Task DisposeAsync()
    {
        await _cache.Remove("databaseUsers");
        await _cache.Remove("databaseUserRoles");
    }

    [Fact]
    public async Task GetConnection_WithTrustedConnection_ReturnsConnectionWithIntegratedSecurity()
    {
        // Arrange
        // Create a database with trusted connection enabled
        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerID,
            _databaseTypeId,
            Status.Active,
            true // Enable trusted connection
        );

        // Add a tenant and create a connection
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName);
        await _dbLocator.AddConnection(tenantId, databaseId);

        // Act
        var connection = await _dbLocator.GetConnection(tenantId, _databaseTypeId);

        // Assert
        Assert.NotNull(connection);
        var connectionString = connection.ConnectionString;
        Assert.Contains("Integrated Security=True", connectionString);
        Assert.DoesNotContain("User ID=", connectionString);
        Assert.DoesNotContain("Password=", connectionString);
    }

    [Fact]
    public async Task GetConnection_WithoutTrustedConnection_ReturnsConnectionWithUserCredentials()
    {
        // Arrange
        // Create a database with trusted connection disabled
        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerID,
            _databaseTypeId,
            Status.Active,
            false // Disable trusted connection
        );

        // Add a tenant and create a connection
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName);
        await _dbLocator.AddConnection(tenantId, databaseId);

        // Act
        var connection = await _dbLocator.GetConnection(tenantId, _databaseTypeId);

        // Assert
        Assert.NotNull(connection);
        var connectionString = connection.ConnectionString;
        Assert.Contains("Integrated Security=False", connectionString);
        Assert.Contains("User ID=", connectionString);
        Assert.Contains("Password=", connectionString);
    }

    [Fact]
    public async Task UpdateDatabase_CanToggleTrustedConnection()
    {
        // Arrange
        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerID,
            _databaseTypeId,
            Status.Active,
            false // Start with trusted connection disabled
        );

        // Act - Enable trusted connection
        await _dbLocator.UpdateDatabase(databaseId, true);

        // Assert
        var database = await _dbLocator.GetDatabase(databaseId);
        Assert.True(database.UseTrustedConnection);

        // Act - Disable trusted connection
        await _dbLocator.UpdateDatabase(databaseId, false);

        // Assert
        database = await _dbLocator.GetDatabase(databaseId);
        Assert.False(database.UseTrustedConnection);
    }
} 