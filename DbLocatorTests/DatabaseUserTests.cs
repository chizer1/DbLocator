using DbLocator;
using DbLocator.Domain;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseUserTests(DbLocatorFixture dbLocatorFixture)
{
    private readonly Locator _dbLocator = dbLocatorFixture.DbLocator;
    private readonly DbLocatorCache _cache = dbLocatorFixture.LocatorCache;
    private readonly int _databaseServerId = dbLocatorFixture.LocalhostServerId;

    [Fact]
    public async Task UseConnection()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        var connectionId = await _dbLocator.AddConnection(tenantId, databaseId);
        var dbUserId = await _dbLocator.AddDatabaseUser(
            [databaseId],
            TestHelpers.GetRandomString(),
            true
        );

        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.DdlAdmin, true);
        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.DataWriter, true);
        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.DataReader, true);
        var connection = await _dbLocator.GetConnection(
            connectionId,
            [DatabaseRole.DdlAdmin, DatabaseRole.DataWriter, DatabaseRole.DataReader]
        );
        Assert.NotNull(connection);
        using var sqlConnection = connection;
        await sqlConnection.OpenAsync();
        using var command = sqlConnection.CreateCommand();
        command.CommandText =
            @"
                    create table test_table (id int primary key);
                    insert into test_table (id) values (0);
                    select * from test_table";
        await sqlConnection.CloseAsync();
    }

    [Fact]
    public async Task NoConnectionsAtFirst()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        var connectionId = await _dbLocator.AddConnection(tenantId, databaseId);
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.GetConnection(connectionId, [DatabaseRole.DataReader])
        );
    }

    [Fact]
    public async Task CanRemoveRoleFromUser()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        var connectionId = await _dbLocator.AddConnection(tenantId, databaseId);
        var dbUserId = await _dbLocator.AddDatabaseUser(
            [databaseId],
            TestHelpers.GetRandomString(),
            true
        );
        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.DataWriter, true);
        var connection = await _dbLocator.GetConnection(connectionId, [DatabaseRole.DataWriter]);
        Assert.NotNull(connection);
        await _dbLocator.DeleteDatabaseUserRole(dbUserId, DatabaseRole.DataWriter, true);
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.GetConnection(connectionId, [DatabaseRole.DataWriter])
        );
    }

    [Fact]
    public async Task VerifyDatabaseUsersAreCached()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        var connectionId = await _dbLocator.AddConnection(tenantId, databaseId);
        var dbUserId = await _dbLocator.AddDatabaseUser(
            [databaseId],
            TestHelpers.GetRandomString(),
            true
        );

        var databaseUsers = await _dbLocator.GetDatabaseUsers();
        Assert.Contains(databaseUsers, db => db.Id == dbUserId);

        var cachedDatabaseUsers = await _cache.GetCachedData<List<DatabaseUser>>("databaseUsers");
        Assert.NotNull(cachedDatabaseUsers);
        Assert.Contains(cachedDatabaseUsers, db => db.Id == dbUserId);
    }
}
