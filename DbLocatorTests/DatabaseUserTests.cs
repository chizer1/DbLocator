using DbLocator;
using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseUserTests : IAsyncLifetime
{
    private readonly Locator _dbLocator;
    private readonly DbLocatorCache _cache;
    private readonly int _databaseServerID;
    private readonly byte _databaseTypeId;
    private readonly int _databaseId;
    private readonly string _databaseName;
    private readonly List<DatabaseUser> _testUsers = new();
    private readonly DbLocatorFixture _fixture;

    public DatabaseUserTests(DbLocatorFixture fixture)
    {
        _fixture = fixture;
        _dbLocator = fixture.DbLocator;
        _databaseServerID = fixture.LocalhostServerId;
        _cache = fixture.LocatorCache;
        _databaseTypeId = _dbLocator.CreateDatabaseType(TestHelpers.GetRandomString()).Result;
        _databaseName = TestHelpers.GetRandomString();
        _databaseId = _dbLocator
            .CreateDatabase(_databaseName, _databaseServerID, _databaseTypeId, Status.Active)
            .Result;
    }

    public async Task InitializeAsync()
    {
        await _cache.Remove("databaseUsers");
        await _cache.Remove("databaseUserRoles");
    }

    public async Task DisposeAsync()
    {
        foreach (var user in _testUsers)
        {
            try
            {
                var roles = (await _dbLocator.GetDatabaseUser(user.Id)).Roles;
                foreach (var role in roles)
                {
                    await _dbLocator.DeleteDatabaseUserRole(user.Id, role);
                }
                await _dbLocator.DeleteDatabaseUser(user.Id, true);
            }
            catch { }
        }
        _testUsers.Clear();
        await _cache.Remove("databaseUsers");
        await _cache.Remove("databaseUserRoles");
    }

    private async Task<DatabaseUser> CreateDatabaseUserAsync(
        string userName = null,
        bool withRoles = false,
        int[] databaseIds = null
    )
    {
        userName ??= TestHelpers.GetRandomString();
        databaseIds ??= [_databaseId];
        var userId = await _dbLocator.CreateDatabaseUser(
            databaseIds,
            userName,
            "TestPassword123!",
            true
        );
        var user = (await _dbLocator.GetDatabaseUsers()).Single(u => u.Id == userId);
        
        if (withRoles)
        {
            await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.Owner);
            await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.SecurityAdmin);
        }
        
        _testUsers.Add(user);
        return user;
    }

    private async Task<Database> CreateDatabaseAsync(string databaseName)
    {
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _databaseServerID,
            _databaseTypeId,
            Status.Active
        );
        return (await _dbLocator.GetDatabases()).Single(db => db.Id == databaseId);
    }

    #region Creation Tests
    [Fact]
    public async Task CreateDatabaseUser_CreatesDatabaseUserDatabaseEntities()
    {
        var user = await CreateDatabaseUserAsync();

        var dbContext = DbContextFactory
            .CreateDbContextFactory(_fixture.ConnectionString)
            .CreateDbContext();
        var databaseUserDatabase = await dbContext
            .Set<DatabaseUserDatabaseEntity>()
            .FirstOrDefaultAsync(d => d.DatabaseUserId == user.Id);

        Assert.NotNull(databaseUserDatabase);
        Assert.True(databaseUserDatabase.DatabaseUserDatabaseId > 0);
        Assert.Equal(user.Id, databaseUserDatabase.DatabaseUserId);
        Assert.Equal(_databaseId, databaseUserDatabase.DatabaseId);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task CreateDatabaseUser_WithDatabaseIds_CreatesUser(bool withRoles)
    {
        var user = await CreateDatabaseUserAsync(withRoles: withRoles);

        var users = await _dbLocator.GetDatabaseUsers();
        Assert.Contains(users, u => u.Name == user.Name);

        var cachedUsers = await _cache.GetCachedData<List<DatabaseUser>>("databaseUsers");
        Assert.NotNull(cachedUsers);
        Assert.Contains(cachedUsers, u => u.Name == user.Name);
        Assert.Equal(_databaseId, user.Databases[0].Id);

        if (withRoles)
        {
            Assert.Contains(DatabaseRole.Owner, user.Roles);
            Assert.Contains(DatabaseRole.SecurityAdmin, user.Roles);
        }
    }

    [Fact]
    public async Task CreateDatabaseUser_WithMultipleDatabases_CreatesCorrectEntities()
    {
        var secondDatabase = await CreateDatabaseAsync(TestHelpers.GetRandomString());
        var user = await CreateDatabaseUserAsync(databaseIds: [_databaseId, secondDatabase.Id]);

        var retrievedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Equal(2, retrievedUser.Databases.Count);
        Assert.Contains(retrievedUser.Databases, d => d.Id == _databaseId);
        Assert.Contains(retrievedUser.Databases, d => d.Id == secondDatabase.Id);
    }
    #endregion

    #region Validation Tests
    [Fact]
    public async Task CannotCreateUserWithInvalidDatabaseId()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.CreateDatabaseUser(
                    [-1],
                    TestHelpers.GetRandomString(),
                    "TestPassword123!",
                    true
                )
        );
    }

    [Fact]
    public async Task CannotCreateUserWithDuplicateName()
    {
        var userName = TestHelpers.GetRandomString();
        await CreateDatabaseUserAsync(userName);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.CreateDatabaseUser(
                    [_databaseId],
                    userName,
                    "TestPassword123!",
                    true
                )
        );
    }

    [Fact]
    public async Task CannotCreateDuplicateRole()
    {
        var user = await CreateDatabaseUserAsync();
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.Owner);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.Owner)
        );
    }
    #endregion

    #region Update Tests
    [Fact]
    public async Task UpdateDatabaseUser_WithAllParameters_UpdatesCorrectly()
    {
        var user = await CreateDatabaseUserAsync();
        var newUserName = TestHelpers.GetRandomString();
        var newPassword = "NewPassword123!";
        var newDatabase = await CreateDatabaseAsync(TestHelpers.GetRandomString());

        await _dbLocator.UpdateDatabaseUser(
            user.Id,
            newUserName,
            newPassword,
            [newDatabase.Id],
            true
        );

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Equal(newUserName, updatedUser.Name);
        Assert.Equal(newDatabase.Id, updatedUser.Databases[0].Id);
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithSameValues_DoesNotUpdate()
    {
        var user = await CreateDatabaseUserAsync();
        var originalName = user.Name;
        var originalDatabases = user.Databases.Select(d => d.Id).ToArray();

        await _dbLocator.UpdateDatabaseUser(
            user.Id,
            originalName,
            "TestPassword123!",
            originalDatabases,
            true
        );

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Equal(originalName, updatedUser.Name);
        Assert.Equal(originalDatabases, updatedUser.Databases.Select(d => d.Id).ToArray());
    }
    #endregion

    #region Delete Tests
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteDatabaseUser_WithDeleteDatabaseUserFlag_HandlesCorrectly(bool deleteDatabaseUser)
    {
        var user = await CreateDatabaseUserAsync();
        await _dbLocator.DeleteDatabaseUser(user.Id, deleteDatabaseUser);

        var users = await _dbLocator.GetDatabaseUsers();
        Assert.DoesNotContain(users, u => u.Id == user.Id);

        var cachedUsers = await _cache.GetCachedData<List<DatabaseUser>>("databaseUsers");
        Assert.DoesNotContain(cachedUsers, u => u.Id == user.Id);

        var dbContext = DbContextFactory
            .CreateDbContextFactory(_fixture.ConnectionString)
            .CreateDbContext();
        var databaseUserDatabase = await dbContext
            .Set<DatabaseUserDatabaseEntity>()
            .FirstOrDefaultAsync(d => d.DatabaseUserId == user.Id);

        if (deleteDatabaseUser)
        {
            Assert.Null(databaseUserDatabase);
        }
        else
        {
            Assert.NotNull(databaseUserDatabase);
        }
    }
    #endregion
}
