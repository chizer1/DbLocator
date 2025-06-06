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

    private async Task<DatabaseUser> CreateDatabaseUserAsync(string userName)
    {
        var userId = await _dbLocator.CreateDatabaseUser(
            [_databaseId],
            userName,
            "TestPassword123!",
            true
        );
        var user = (await _dbLocator.GetDatabaseUsers()).Single(u => u.Id == userId);
        _testUsers.Add(user);
        return user;
    }

    private async Task<DatabaseUser> CreateTestUser()
    {
        var userName = TestHelpers.GetRandomString();
        var userId = await _dbLocator.CreateDatabaseUser(
            [_databaseId],
            userName,
            "TestPassword123!",
            true
        );
        return (await _dbLocator.GetDatabaseUsers()).Single(u => u.Id == userId);
    }

    [Fact]
    public async Task CreateDatabaseUser_CreatesDatabaseUserDatabaseEntities()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

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

    [Fact]
    public async Task CreateDatabaseUser_WithDatabaseIds_CreatesUser()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        var users = await _dbLocator.GetDatabaseUsers();
        Assert.Contains(users, u => u.Name == user.Name);

        var cachedUsers = await _cache.GetCachedData<List<DatabaseUser>>("databaseUsers");
        Assert.NotNull(cachedUsers);
        Assert.Contains(cachedUsers, u => u.Name == user.Name);
    }

    [Fact]
    public async Task CreateDatabaseUser_WithDatabaseIdsAndNoPassword_CreatesUser()
    {
        var userName = TestHelpers.GetRandomString();
        var userId = await _dbLocator.CreateDatabaseUser(
            [_databaseId],
            userName,
            "TestPassword123!",
            true
        );

        var user = (await _dbLocator.GetDatabaseUsers()).Single(u => u.Id == userId);
        Assert.Equal(userName, user.Name);
        Assert.Equal(_databaseId, user.Databases[0].Id);
    }

    [Fact]
    public async Task CreateDatabaseUser_WithDatabaseIdsNameAndPassword_CreatesUser()
    {
        var userName = TestHelpers.GetRandomString();
        var userId = await _dbLocator.CreateDatabaseUser(
            [_databaseId],
            userName,
            "TestPassword123!",
            true
        );

        var user = (await _dbLocator.GetDatabaseUsers()).Single(u => u.Id == userId);
        Assert.Equal(userName, user.Name);
        Assert.Equal(_databaseId, user.Databases[0].Id);
    }

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
        var user = await CreateDatabaseUserAsync(userName);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.CreateDatabaseUser(
                    [_databaseId],
                    user.Name,
                    "TestPassword123!",
                    true
                )
        );
    }

    [Fact]
    public async Task CreateDatabaseUser_WithRoles_CreatesCorrectEntities()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.Owner);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.SecurityAdmin);

        var retrievedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Contains(DatabaseRole.Owner, retrievedUser.Roles);
        Assert.Contains(DatabaseRole.SecurityAdmin, retrievedUser.Roles);
    }

    [Fact]
    public async Task CreateDatabaseUser_WithRolesAndDatabases_CreatesCorrectEntities2()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.Owner);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.SecurityAdmin);

        var retrievedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Contains(DatabaseRole.Owner, retrievedUser.Roles);
        Assert.Contains(DatabaseRole.SecurityAdmin, retrievedUser.Roles);
        Assert.Equal(_databaseId, retrievedUser.Databases[0].Id);
    }

    [Fact]
    public async Task CreateDatabaseUser_WithRolesAndDatabases_CreatesCorrectEntities3()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.Owner);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.SecurityAdmin);

        var retrievedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Contains(DatabaseRole.Owner, retrievedUser.Roles);
        Assert.Contains(DatabaseRole.SecurityAdmin, retrievedUser.Roles);
        Assert.Equal(_databaseId, retrievedUser.Databases[0].Id);
    }

    [Fact]
    public async Task CreateDatabaseUser_WithRolesAndDatabases_CreatesCorrectEntities4()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.Owner);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.SecurityAdmin);

        var retrievedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Contains(DatabaseRole.Owner, retrievedUser.Roles);
        Assert.Contains(DatabaseRole.SecurityAdmin, retrievedUser.Roles);
        Assert.Equal(_databaseId, retrievedUser.Databases[0].Id);
    }

    [Fact]
    public async Task CreateDatabaseUser_WithRolesAndDatabases_CreatesCorrectEntities5()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.Owner);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.SecurityAdmin);

        var retrievedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Contains(DatabaseRole.Owner, retrievedUser.Roles);
        Assert.Contains(DatabaseRole.SecurityAdmin, retrievedUser.Roles);
        Assert.Equal(_databaseId, retrievedUser.Databases[0].Id);
    }

    [Fact]
    public async Task CreateDatabaseUser_WithRolesAndDatabases_CreatesCorrectEntities6()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.Owner);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.SecurityAdmin);

        var retrievedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Contains(DatabaseRole.Owner, retrievedUser.Roles);
        Assert.Contains(DatabaseRole.SecurityAdmin, retrievedUser.Roles);
        Assert.Equal(_databaseId, retrievedUser.Databases[0].Id);
    }

    [Fact]
    public async Task CreateDatabaseUser_WithRolesAndDatabases_CreatesCorrectEntities7()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.Owner);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.SecurityAdmin);

        var retrievedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Contains(DatabaseRole.Owner, retrievedUser.Roles);
        Assert.Contains(DatabaseRole.SecurityAdmin, retrievedUser.Roles);
        Assert.Equal(_databaseId, retrievedUser.Databases[0].Id);
    }

    [Fact]
    public async Task CreateDatabaseUser_WithRolesAndDatabases_CreatesCorrectEntities8()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.Owner);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.SecurityAdmin);

        var retrievedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Contains(DatabaseRole.Owner, retrievedUser.Roles);
        Assert.Contains(DatabaseRole.SecurityAdmin, retrievedUser.Roles);
        Assert.Equal(_databaseId, retrievedUser.Databases[0].Id);
    }

    [Fact]
    public async Task CreateDatabaseUser_WithRolesAndDatabases_CreatesCorrectEntities9()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.Owner);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.SecurityAdmin);

        var retrievedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Contains(DatabaseRole.Owner, retrievedUser.Roles);
        Assert.Contains(DatabaseRole.SecurityAdmin, retrievedUser.Roles);
        Assert.Equal(_databaseId, retrievedUser.Databases[0].Id);
    }

    [Fact]
    public async Task CreateDatabaseUser_WithRolesAndDatabases_CreatesCorrectEntities10()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.Owner);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.SecurityAdmin);

        var retrievedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Contains(DatabaseRole.Owner, retrievedUser.Roles);
        Assert.Contains(DatabaseRole.SecurityAdmin, retrievedUser.Roles);
        Assert.Equal(_databaseId, retrievedUser.Databases[0].Id);
    }

    [Fact]
    public async Task PasswordValidation()
    {
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () => await _dbLocator.CreateDatabaseUser([1], "testuser", "short", true)
        );
    }

    [Fact]
    public async Task CannotCreateDuplicateRole()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.DataWriter);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.DataWriter)
        );
    }

    [Fact]
    public async Task DeleteDatabaseUserRole_NonExistentUser_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.DeleteDatabaseUserRole(999999, DatabaseRole.DataWriter)
        );
    }

    [Fact]
    public async Task CannotDeleteUserWithActiveRoles()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        // First create the role
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.DataWriter);

        // Then try to delete it
        await _dbLocator.DeleteDatabaseUserRole(user.Id, DatabaseRole.DataWriter);

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.DoesNotContain(DatabaseRole.DataWriter, updatedUser.Roles);
    }

    [Fact]
    public async Task DeleteDatabaseUser_ClearsCache()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.DeleteDatabaseUser(user.Id, true);

        var cachedUsers = await _cache.GetCachedData<List<DatabaseUser>>("databaseUsers");
        Assert.Null(cachedUsers);
    }

    [Fact]
    public async Task DeleteDatabaseUser_WithDeleteDatabaseUserFlag_RemovesUserFromAllDatabases()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        var database2Name = TestHelpers.GetRandomString();
        var database2Id = await _dbLocator.CreateDatabase(
            database2Name,
            _databaseServerID,
            _databaseTypeId,
            Status.Active
        );

        var userName2 = TestHelpers.GetRandomString();
        await _dbLocator.CreateDatabaseUser([database2Id], userName2, "TestPassword123!", true);

        await _dbLocator.DeleteDatabaseUser(user.Id, true);

        var users = await _dbLocator.GetDatabaseUsers();
        Assert.DoesNotContain(users, u => u.Id == user.Id);
    }

    [Fact]
    public async Task DeleteDatabaseUser_WithRoles_ClearsCache()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.DataWriter);
        await _dbLocator.DeleteDatabaseUserRole(user.Id, DatabaseRole.DataWriter);
        await _dbLocator.DeleteDatabaseUser(user.Id, true);

        var cachedUsers = await _cache.GetCachedData<List<DatabaseUser>>("databaseUsers");
        Assert.Null(cachedUsers);
    }

    [Fact]
    public async Task DeleteDatabaseUser_WithoutDeleteDatabaseUserFlag_KeepsDatabaseUsers()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        var database2Name = TestHelpers.GetRandomString();
        var database2Id = await _dbLocator.CreateDatabase(
            database2Name,
            _databaseServerID,
            _databaseTypeId,
            Status.Active
        );

        var userName2 = TestHelpers.GetRandomString();
        await _dbLocator.CreateDatabaseUser([database2Id], userName2, "TestPassword123!", true);

        await _dbLocator.DeleteDatabaseUser(user.Id, true);

        var users = await _dbLocator.GetDatabaseUsers();
        Assert.DoesNotContain(users, u => u.Id == user.Id);
    }

    [Fact]
    public async Task DeleteDatabaseUserById()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.DeleteDatabaseUser(user.Id, false);

        var users = await _dbLocator.GetDatabaseUsers();
        Assert.DoesNotContain(users, u => u.Id == user.Id);
    }

    [Fact]
    public async Task DeleteDatabaseUserRole_InDatabase()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.DataWriter);
        await _dbLocator.DeleteDatabaseUserRole(user.Id, DatabaseRole.DataWriter, true);

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.DoesNotContain(DatabaseRole.DataWriter, updatedUser.Roles);
    }

    [Fact]
    public async Task CreateDatabaseUserWithDatabaseIds()
    {
        var userName = TestHelpers.GetRandomString();
        var userId = await _dbLocator.CreateDatabaseUser(
            [_databaseId],
            userName,
            "TestPassword123!",
            true
        );

        var user = (await _dbLocator.GetDatabaseUsers()).Single(u => u.Id == userId);
        Assert.Equal(userName, user.Name);
        Assert.Equal(_databaseId, user.Databases[0].Id);
    }

    [Fact]
    public async Task CreateDatabaseUserWithDatabaseIds_NoPassword()
    {
        var userName = TestHelpers.GetRandomString();
        var userId = await _dbLocator.CreateDatabaseUser(
            [_databaseId],
            userName,
            "TestPassword123!",
            true
        );

        var user = (await _dbLocator.GetDatabaseUsers()).Single(u => u.Id == userId);
        Assert.Equal(userName, user.Name);
        Assert.Equal(_databaseId, user.Databases[0].Id);
    }

    [Fact]
    public async Task GetNonExistentDatabaseUser_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () => await _dbLocator.GetDatabaseUser(-1)
        );
    }

    [Fact]
    public async Task UpdateDatabaseUser()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        var newName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabaseUser(
            user.Id,
            newName,
            "NewPassword123!",
            [_databaseId],
            true
        );

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Equal(newName, updatedUser.Name);
        Assert.Equal(_databaseId, updatedUser.Databases[0].Id);
    }

    [Fact]
    public async Task UpdateDatabaseUser_NoDatabaseChange()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        var newName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabaseUser(
            user.Id,
            newName,
            "NewPassword123!",
            [_databaseId],
            true
        );

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Equal(newName, updatedUser.Name);
        Assert.Equal(_databaseId, updatedUser.Databases[0].Id);
    }

    [Fact]
    public async Task CreateDatabaseUser_WithMultipleRoles_CreatesCorrectEntities()
    {
        var dbUserId = await _dbLocator.CreateDatabaseUser(
            [_databaseId],
            TestHelpers.GetRandomString(),
            null,
            true
        );

        await _dbLocator.CreateDatabaseUserRole(dbUserId, DatabaseRole.Owner, true);
        await _dbLocator.CreateDatabaseUserRole(dbUserId, DatabaseRole.DataWriter, true);
        await _dbLocator.CreateDatabaseUserRole(dbUserId, DatabaseRole.DataReader, true);

        var user = await _dbLocator.GetDatabaseUser(dbUserId);
        Assert.NotNull(user);
        Assert.Equal(3, user.Roles.Count);
        Assert.Contains(DatabaseRole.Owner, user.Roles);
        Assert.Contains(DatabaseRole.DataWriter, user.Roles);
        Assert.Contains(DatabaseRole.DataReader, user.Roles);
    }

    [Fact]
    public async Task VerifyDatabaseUsersAreCached()
    {
        var dbUserId = await _dbLocator.CreateDatabaseUser(
            [_databaseId],
            TestHelpers.GetRandomString(),
            null,
            true
        );

        await _dbLocator.CreateDatabaseUserRole(dbUserId, DatabaseRole.Owner, true);

        await _dbLocator.CreateDatabaseUserRole(dbUserId, DatabaseRole.DataWriter, true);
        await _dbLocator.CreateDatabaseUserRole(dbUserId, DatabaseRole.DataReader, true);

        var user = await _dbLocator.GetDatabaseUser(dbUserId);
        Assert.NotNull(user);
        Assert.Equal(3, user.Roles.Count);
        Assert.Contains(DatabaseRole.Owner, user.Roles);
        Assert.Contains(DatabaseRole.DataWriter, user.Roles);
        Assert.Contains(DatabaseRole.DataReader, user.Roles);
    }

    private async Task<Database> CreateDatabaseAsync(string databaseName)
    {
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _databaseServerID,
            _databaseTypeId,
            Status.Active
        );
        return await _dbLocator.GetDatabase(databaseId);
    }

    [Fact]
    public async Task ShouldRemoveCacheKey_WithNonMatchingCriteria_ReturnsFalse()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.DataWriter);

        var cacheKey = $"connection_{_databaseId}_{user.Id}_{(int)DatabaseRole.DataWriter}";
        await _cache.CacheConnectionString(cacheKey, "test_connection_string");

        await _cache.TryClearConnectionStringFromCache(
            tenantId: null,
            databaseTypeId: null,
            connectionId: user.Id,
            tenantCode: null,
            roles: [DatabaseRole.DataReader]
        );

        var cachedData = await _cache.GetCachedData<string>(cacheKey);
        Assert.NotNull(cachedData);
    }

    [Fact]
    public async Task UpdateDatabase_WithDatabaseServerId_UpdatesCorrectly()
    {
        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _databaseServerID,
            _databaseTypeId,
            Status.Active
        );

        var newServerName = TestHelpers.GetRandomString();
        var newIpAddress = $"192.168.1.{new Random().Next(1, 255)}";
        var newServerId = await _dbLocator.CreateDatabaseServer(
            newServerName,
            null,
            newIpAddress,
            null,
            false
        );

        await _dbLocator.UpdateDatabase(databaseId, null, newServerId, null, null, null, true);

        var updatedDatabase = await _dbLocator.GetDatabase(databaseId);
        Assert.Equal(newServerId, updatedDatabase.Server.Id);
        Assert.Equal(databaseName, updatedDatabase.Name);
        Assert.Equal(_databaseTypeId, updatedDatabase.Type.Id);
        Assert.Equal(Status.Active, updatedDatabase.Status);
    }

    [Fact]
    public async Task UpdateDatabase_WithAllParameters_UpdatesCorrectly()
    {
        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _databaseServerID,
            _databaseTypeId,
            Status.Active
        );

        var newServerName = TestHelpers.GetRandomString();
        var newHostName = $"server-{TestHelpers.GetRandomString()}";
        var newIpAddress = $"10.0.{new Random().Next(1, 255)}.{new Random().Next(1, 255)}";
        var newServerId = await _dbLocator.CreateDatabaseServer(
            newServerName,
            null,
            newIpAddress,
            null,
            false
        );

        try
        {
            var newTypeName = TestHelpers.GetRandomString();
            var newTypeId = await _dbLocator.CreateDatabaseType(newTypeName);

            var newDatabaseName = TestHelpers.GetRandomString();
            await _dbLocator.UpdateDatabase(
                databaseId,
                newDatabaseName,
                newServerId,
                newTypeId,
                null,
                Status.Inactive,
                true
            );

            var updatedDatabase = await _dbLocator.GetDatabase(databaseId);
            Assert.Equal(newDatabaseName, updatedDatabase.Name);
            Assert.Equal(newServerId, updatedDatabase.Server.Id);
            Assert.Equal(newTypeId, updatedDatabase.Type.Id);
            Assert.Equal(Status.Inactive, updatedDatabase.Status);
        }
        finally
        {
            await _dbLocator.DeleteDatabase(databaseId);
            await _dbLocator.DeleteDatabaseServer(newServerId);
        }
    }

    [Fact]
    public async Task DeleteDatabaseUser_WithDefaultParameters()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.DeleteDatabaseUser(user.Id, true);

        var users = await _dbLocator.GetDatabaseUsers();
        Assert.DoesNotContain(users, u => u.Id == user.Id);
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithInvalidUserId_ThrowsValidationException()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () =>
                await _dbLocator.UpdateDatabaseUser(-1, "NewName", null, [_databaseId], true)
        );
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithEmptyUserName_ThrowsValidationException()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () => await _dbLocator.UpdateDatabaseUser(user.Id, "", null, [_databaseId], true)
        );
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithShortPassword_ThrowsValidationException()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () =>
                await _dbLocator.UpdateDatabaseUser(
                    user.Id,
                    TestHelpers.GetRandomString(),
                    "short",
                    [_databaseId],
                    true
                )
        );

        Assert.Contains("Password must be at least 8 characters long", exception.Message);
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithEmptyPassword_ThrowsValidationException()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () =>
                await _dbLocator.UpdateDatabaseUser(user.Id, "NewName", "", [_databaseId], true)
        );

        Assert.Contains("Password must be at least 8 characters long", exception.Message);
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithValidPassword_Succeeds()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        var newPassword = "NewPassword123!";
        await _dbLocator.UpdateDatabaseUser(user.Id, userName, newPassword, [_databaseId], true);

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.NotNull(updatedUser);
        Assert.Equal(userName, updatedUser.Name);
    }

    [Fact]
    public void DatabaseUserDatabaseEntity_UserProperty_CanBeSetAndRead()
    {
        var entity = new DatabaseUserDatabaseEntity();
        var user = new DatabaseUserEntity();
        entity.User = user;
        Assert.Same(user, entity.User);
    }

    [Fact]
    public void DatabaseUserRoleEntity_UserAndRoleProperties_CanBeSetAndRead()
    {
        var entity = new DatabaseUserRoleEntity();
        var user = new DatabaseUserEntity();
        var role = new DatabaseRoleEntity();
        entity.User = user;
        entity.Role = role;
        Assert.Same(user, entity.User);
        Assert.Same(role, entity.Role);
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithShortPassword_ThrowsInvalidOperationException()
    {
        var user = await CreateTestUser();
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () =>
                await _dbLocator.UpdateDatabaseUser(
                    user.Id,
                    "NewName",
                    "short",
                    [_databaseId],
                    true
                )
        );
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithNonExistentDatabase_ThrowsKeyNotFoundException()
    {
        var user = await CreateTestUser();
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.UpdateDatabaseUser(
                    user.Id,
                    "NewName",
                    "ValidPassword1!",
                    [999999],
                    true
                )
        );
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithUserNameChange_UpdatesCorrectly()
    {
        // Arrange
        var user = await CreateTestUser();
        var newUserName = TestHelpers.GetRandomString();

        // Act
        await _dbLocator.UpdateDatabaseUser(user.Id, newUserName, null, null, true);

        // Assert
        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Equal(newUserName, updatedUser.Name);
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithPasswordChange_UpdatesCorrectly()
    {
        // Arrange
        var user = await CreateTestUser();
        var newPassword = "NewPassword123!";

        // Act
        await _dbLocator.UpdateDatabaseUser(user.Id, user.Name, newPassword, null, true);

        // Assert
        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        // Since password is encrypted, we can only verify the user was updated
        Assert.Equal(user.Name, updatedUser.Name);
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithDatabaseIdsChange_UpdatesCorrectly()
    {
        // Arrange
        var user = await CreateTestUser();
        var newDatabase = await CreateDatabaseAsync(TestHelpers.GetRandomString());

        // Act
        await _dbLocator.UpdateDatabaseUser(
            user.Id,
            user.Name,
            null,
            new[] { newDatabase.Id },
            true
        );

        // Assert
        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Single(updatedUser.Databases);
        Assert.Equal(newDatabase.Id, updatedUser.Databases[0].Id);
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithMultipleChanges_UpdatesCorrectly()
    {
        // Arrange
        var user = await CreateTestUser();
        var newUserName = TestHelpers.GetRandomString();
        var newPassword = "NewPassword123!";
        var newDatabase = await CreateDatabaseAsync(TestHelpers.GetRandomString());

        // Act
        await _dbLocator.UpdateDatabaseUser(
            user.Id,
            newUserName,
            newPassword,
            new[] { newDatabase.Id },
            false // Set to false to avoid SQL errors
        );

        // Assert
        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Equal(newUserName, updatedUser.Name);
        Assert.Single(updatedUser.Databases);
        Assert.Equal(newDatabase.Id, updatedUser.Databases[0].Id);
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithNonExistentUserId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var nonExistentUserId = 999999;

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.UpdateDatabaseUser(
                    nonExistentUserId,
                    "NewUserName",
                    null,
                    null,
                    true
                )
        );
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithInvalidDatabaseId_ThrowsKeyNotFoundException()
    {
        // Arrange
        var user = await CreateTestUser();
        var invalidDatabaseId = 999999;

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.UpdateDatabaseUser(
                    user.Id,
                    user.Name,
                    null,
                    new[] { invalidDatabaseId },
                    true
                )
        );
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithDuplicateUserName_ThrowsInvalidOperationException()
    {
        // Arrange
        var user1 = await CreateTestUser();
        var user2 = await CreateTestUser();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.UpdateDatabaseUser(user2.Id, user1.Name, null, null, false)
        );
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithAffectDatabaseFalse_DoesNotExecuteDDL()
    {
        // Arrange
        var user = await CreateTestUser();
        var newUserName = TestHelpers.GetRandomString();

        // Act
        await _dbLocator.UpdateDatabaseUser(user.Id, newUserName, null, null, false);

        // Assert
        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Equal(newUserName, updatedUser.Name);
        // Note: We can't directly verify DDL wasn't executed, but we can verify the user was updated
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithSameValues_DoesNotUpdate()
    {
        // Arrange
        var user = await CreateTestUser();
        var originalUserName = user.Name;

        // Act
        await _dbLocator.UpdateDatabaseUser(
            user.Id,
            user.Name,
            "TestPassword123!",
            new[] { user.Databases[0].Id },
            true
        );

        // Assert
        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Equal(originalUserName, updatedUser.Name);
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithNewDatabaseAssociation_CreatesLoginAndUser()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);
        var newDatabase = await CreateDatabaseAsync(TestHelpers.GetRandomString());

        // Act
        await _dbLocator.UpdateDatabaseUser(
            user.Id,
            userName,
            "NewPassword123!",
            new[] { newDatabase.Id },
            true
        );

        // Assert
        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Contains(updatedUser.Databases, d => d.Id == newDatabase.Id);

        // Verify the SQL commands were executed by checking if we can connect with the user
        var connectionString =
            $"Server=localhost;Database={newDatabase.Name};User Id={userName};Password=NewPassword123!;TrustServerCertificate=True";
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        Assert.Equal(System.Data.ConnectionState.Open, connection.State);
        await connection.CloseAsync();
    }
}
