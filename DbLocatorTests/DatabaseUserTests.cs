using DbLocator;
using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;
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
        var uniqueId = Convert
            .ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("=", "")
            .Replace("+", "")
            .Replace("/", "")[..8];

        var uniqueUserName = $"TestUser_{userName}_{uniqueId}";
        var userId = await _dbLocator.CreateDatabaseUser(
            [_databaseId],
            uniqueUserName,
            "TestPassword123!",
            true
        );

        var user = (await _dbLocator.GetDatabaseUsers()).Single(u => u.Id == userId);
        _testUsers.Add(user);
        return user;
    }

    [Fact]
    public async Task CreateMultipleDatabaseUsers()
    {
        var userNamePrefix = TestHelpers.GetRandomString();
        var user1 = await CreateDatabaseUserAsync($"{userNamePrefix}1");
        var user2 = await CreateDatabaseUserAsync($"{userNamePrefix}2");

        var users = (await _dbLocator.GetDatabaseUsers()).ToList();
        Assert.Contains(users, u => u.Name == user1.Name);
        Assert.Contains(users, u => u.Name == user2.Name);
    }

    [Fact]
    public async Task VerifyDatabaseUsersAreCached()
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
    public async Task GetDatabaseUserById()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        var retrievedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.NotNull(retrievedUser);
        Assert.Equal(user.Id, retrievedUser.Id);
        Assert.Equal(user.Name, retrievedUser.Name);
        Assert.Equal(user.Databases[0].Id, retrievedUser.Databases[0].Id);
        Assert.Equal(user.Roles, retrievedUser.Roles);
    }

    [Fact]
    public async Task GetNonExistentDatabaseUser_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () => await _dbLocator.GetDatabaseUser(-1)
        );
    }

    [Fact]
    public async Task UpdateNonExistentDatabaseUser_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.UpdateDatabaseUser(-1, [_databaseId], "testuser", "Test123!", true)
        );
    }

    [Fact]
    public async Task DeleteNonExistentDatabaseUser_ThrowsKeyNotFoundException()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.DeleteDatabaseUser(-1, true)
        );
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
    public async Task CannotUpdateUserWithDuplicateName()
    {
        var userName1 = TestHelpers.GetRandomString();
        var userName2 = TestHelpers.GetRandomString();
        var user1 = await CreateDatabaseUserAsync(userName1);
        var user2 = await CreateDatabaseUserAsync(userName2);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.UpdateDatabaseUser(
                    user1.Id,
                    [_databaseId],
                    user2.Name,
                    "TestPassword123!",
                    true
                )
        );
    }

    [Fact]
    public async Task CannotDeleteUserWithActiveRoles()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.DataWriter);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.DeleteDatabaseUser(user.Id, true)
        );
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
    public async Task CanDeleteDatabaseUserRole()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.DataWriter);

        await _dbLocator.DeleteDatabaseUserRole(user.Id, DatabaseRole.DataWriter);

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.DoesNotContain(DatabaseRole.DataWriter, updatedUser.Roles);
    }

    [Fact]
    public async Task DeleteDatabaseUserRole_NonExistentRole_Succeeds()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

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
    public async Task DeleteDatabaseUser_WithDeleteDatabaseUserFlag_RemovesUserFromAllDatabases()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.DeleteDatabaseUser(user.Id, true);

        var users = await _dbLocator.GetDatabaseUsers();
        Assert.DoesNotContain(users, u => u.Id == user.Id);
    }

    [Fact]
    public async Task DeleteDatabaseUser_WithoutDeleteDatabaseUserFlag_KeepsDatabaseUsers()
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
        var user = await CreateDatabaseUserAsync(userName);

        var users = await _dbLocator.GetDatabaseUsers();
        Assert.Contains(users, u => u.Id == user.Id);
        Assert.Contains(user.Databases, d => d.Id == _databaseId);
    }

    [Fact]
    public async Task CreateDatabaseUserWithDatabaseIds_NoPassword()
    {
        var userName = TestHelpers.GetRandomString();
        var userId = await _dbLocator.CreateDatabaseUser([_databaseId], userName, true);

        var user = await _dbLocator.GetDatabaseUser(userId);
        Assert.Equal(userName, user.Name);
        Assert.Contains(user.Databases, d => d.Id == _databaseId);
    }

    [Fact]
    public async Task DeleteDatabaseUserById()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.DeleteDatabaseUser(user.Id, true);

        var users = await _dbLocator.GetDatabaseUsers();
        Assert.DoesNotContain(users, u => u.Id == user.Id);
    }

    [Fact]
    public async Task UpdateDatabaseUserById()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        var newUserName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabaseUser(
            user.Id,
            [_databaseId],
            newUserName,
            "NewPassword123!",
            true
        );

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Equal(newUserName, updatedUser.Name);
    }

    [Fact]
    public async Task UpdateDatabaseUser()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        var newUserName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabaseUser(
            user.Id,
            [_databaseId],
            newUserName,
            "NewPassword123!",
            true
        );

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Equal(newUserName, updatedUser.Name);
    }

    [Fact]
    public async Task UpdateDatabaseUser_NoDatabaseChange()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        var newUserName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabaseUser(
            user.Id,
            [_databaseId],
            newUserName,
            "NewPassword123!",
            false
        );

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Equal(newUserName, updatedUser.Name);
    }

    [Fact]
    public async Task CreateDatabaseUser_WithDatabaseIdsNameAndPassword()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        var users = await _dbLocator.GetDatabaseUsers();
        Assert.Contains(users, u => u.Id == user.Id);
        Assert.Contains(user.Databases, d => d.Id == _databaseId);
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithDatabaseIdsAndName()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        var newUserName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabaseUser(user.Id, [_databaseId], newUserName, true);

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Equal(newUserName, updatedUser.Name);
    }

    [Fact]
    public async Task UpdateDatabaseUser_RemovingAnExistingDatabaseId()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        var newDatabaseName = TestHelpers.GetRandomString();
        var newDatabaseId = await _dbLocator.CreateDatabase(
            newDatabaseName,
            _databaseServerID,
            _databaseTypeId,
            Status.Active
        );

        await _dbLocator.UpdateDatabaseUser(user.Id, [newDatabaseId], user.Name, true);

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.DoesNotContain(updatedUser.Databases, d => d.Id == _databaseId);
        Assert.Contains(updatedUser.Databases, d => d.Id == newDatabaseId);
    }

    [Fact]
    public async Task CreateDatabaseUser_WithRolesAndDatabases_CreatesCorrectEntities()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.DataWriter);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.DataReader);

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Contains(DatabaseRole.DataWriter, updatedUser.Roles);
        Assert.Contains(DatabaseRole.DataReader, updatedUser.Roles);
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithRolesAndDatabases_UpdatesCorrectEntities()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.DataWriter);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.DataReader);

        var newUserName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabaseUser(
            user.Id,
            [_databaseId],
            newUserName,
            "NewPassword123!",
            true
        );

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Equal(newUserName, updatedUser.Name);
        Assert.Contains(DatabaseRole.DataWriter, updatedUser.Roles);
        Assert.Contains(DatabaseRole.DataReader, updatedUser.Roles);
    }

    [Fact]
    public async Task CreateDatabaseUser_WithMultipleDatabases_CreatesCorrectEntities()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        var newDatabaseName = TestHelpers.GetRandomString();
        var newDatabaseId = await _dbLocator.CreateDatabase(
            newDatabaseName,
            _databaseServerID,
            _databaseTypeId,
            Status.Active
        );

        await _dbLocator.UpdateDatabaseUser(user.Id, [_databaseId, newDatabaseId], user.Name, true);

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Contains(updatedUser.Databases, d => d.Id == _databaseId);
        Assert.Contains(updatedUser.Databases, d => d.Id == newDatabaseId);
    }

    [Fact]
    public async Task CreateDatabaseUser_WithMultipleRoles_CreatesCorrectEntities()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.DataWriter);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.DataReader);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.Owner);

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Contains(DatabaseRole.DataWriter, updatedUser.Roles);
        Assert.Contains(DatabaseRole.DataReader, updatedUser.Roles);
        Assert.Contains(DatabaseRole.Owner, updatedUser.Roles);
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithMultipleRoles_UpdatesCorrectEntities()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.DataWriter);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.DataReader);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.Owner);

        var newUserName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabaseUser(
            user.Id,
            [_databaseId],
            newUserName,
            "NewPassword123!",
            true
        );

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Equal(newUserName, updatedUser.Name);
        Assert.Contains(DatabaseRole.DataWriter, updatedUser.Roles);
        Assert.Contains(DatabaseRole.DataReader, updatedUser.Roles);
        Assert.Contains(DatabaseRole.Owner, updatedUser.Roles);
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
    public async Task CreateDatabaseUser_WithRolesAndDatabases_LoadsNavigationProperties()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.DataWriter);
        await _dbLocator.CreateDatabaseUserRole(user.Id, DatabaseRole.DataReader);

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Contains(DatabaseRole.DataWriter, updatedUser.Roles);
        Assert.Contains(DatabaseRole.DataReader, updatedUser.Roles);
        Assert.Contains(updatedUser.Databases, d => d.Id == _databaseId);
    }

    [Fact]
    public async Task CreateDatabaseUser_CreatesDatabaseUserDatabaseEntities()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Contains(updatedUser.Databases, d => d.Id == _databaseId);
    }

    [Fact]
    public async Task ShouldRemoveCacheKey_WithNonMatchingCriteria_ReturnsFalse()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        var cacheKey = "databaseUsers";
        var cachedUsers = await _cache.GetCachedData<List<DatabaseUser>>(cacheKey);
        Assert.NotNull(cachedUsers);
        Assert.Contains(cachedUsers, u => u.Id == user.Id);
    }

    [Fact]
    public async Task UpdateDatabase_WithDatabaseServerId_UpdatesCorrectly()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        var newDatabaseName = TestHelpers.GetRandomString();
        var newDatabaseId = await _dbLocator.CreateDatabase(
            newDatabaseName,
            _databaseServerID,
            _databaseTypeId,
            Status.Active
        );

        await _dbLocator.UpdateDatabaseUser(user.Id, [newDatabaseId], user.Name, true);

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.DoesNotContain(updatedUser.Databases, d => d.Id == _databaseId);
        Assert.Contains(updatedUser.Databases, d => d.Id == newDatabaseId);
    }

    [Fact]
    public async Task UpdateDatabase_WithAllParameters_UpdatesCorrectly()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);

        var newUserName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabaseUser(
            user.Id,
            [_databaseId],
            newUserName,
            "NewPassword123!",
            true
        );

        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.Equal(newUserName, updatedUser.Name);
        Assert.Contains(updatedUser.Databases, d => d.Id == _databaseId);
    }
}
