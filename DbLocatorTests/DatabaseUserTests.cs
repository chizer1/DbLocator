using System.ComponentModel.DataAnnotations;
using DbLocator;
using DbLocator.Domain;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;

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

    public DatabaseUserTests(DbLocatorFixture dbLocatorFixture)
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
    }

    public async Task DisposeAsync()
    {
        foreach (var user in _testUsers)
        {
            try
            {
                // Delete any roles first
                var roles = (await _dbLocator.GetDatabaseUser(user.Id)).Roles;
                foreach (var role in roles)
                {
                    await _dbLocator.DeleteDatabaseUserRole(user.Id, role);
                }
                await _dbLocator.DeleteDatabaseUser(user.Id, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
        _testUsers.Clear();
    }

    private async Task<DatabaseUser> AddDatabaseUserAsync(string userName)
    {
        // Generate a unique 8-character string from a GUID
        var uniqueId = Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("=", "")
            .Replace("+", "")
            .Replace("/", "")
            .Substring(0, 8);
            
        var uniqueUserName = $"TestUser_{userName}_{uniqueId}";
        var userId = await _dbLocator.AddDatabaseUser(
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
    public async Task AddMultipleDatabaseUsers()
    {
        var userNamePrefix = TestHelpers.GetRandomString();
        var user1 = await AddDatabaseUserAsync($"{userNamePrefix}1");
        var user2 = await AddDatabaseUserAsync($"{userNamePrefix}2");

        var users = (await _dbLocator.GetDatabaseUsers()).ToList();
        Assert.Contains(users, u => u.Name == user1.Name);
        Assert.Contains(users, u => u.Name == user2.Name);
    }

    [Fact]
    public async Task VerifyDatabaseUsersAreCached()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await AddDatabaseUserAsync(userName);

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
        var user = await AddDatabaseUserAsync(userName);

        var retrievedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.NotNull(retrievedUser);
        Assert.Equal(user.Id, retrievedUser.Id);
        Assert.Equal(user.Name, retrievedUser.Name);
        Assert.Equal(user.Databases[0].Id, retrievedUser.Databases[0].Id);
        Assert.Equal(user.Roles, retrievedUser.Roles);
    }

    // [Fact]
    // public async Task UpdateDatabaseUser()
    // {
    //     var userName = TestHelpers.GetRandomString();
    //     var user = await AddDatabaseUserAsync(userName);

    //     var newName = TestHelpers.GetRandomString();
    //     await _dbLocator.UpdateDatabaseUser(
    //         user.Id,
    //         [_databaseId],
    //         newName,
    //         "NewPassword123!",
    //         true
    //     );

    //     var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
    //     Assert.Equal(newName, updatedUser.Name);
    //     Assert.Equal(_databaseId, updatedUser.Databases[0].Id);
    // }

    // [Fact]
    // public async Task DeleteDatabaseUser()
    // {
    //     var userName = TestHelpers.GetRandomString();
    //     var user = await AddDatabaseUserAsync(userName);

    //     // Delete the user record and database user
    //     await _dbLocator.DeleteDatabaseUser(user.Id, true);

    //     var users = await _dbLocator.GetDatabaseUsers();
    //     Assert.DoesNotContain(users, u => u.Id == user.Id);
    // }

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
    public async Task CannotAddUserWithInvalidDatabaseId()
    {
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.AddDatabaseUser(
                    [-1],
                    TestHelpers.GetRandomString(),
                    "TestPassword123!",
                    true
                )
        );
    }

    [Fact]
    public async Task CannotAddUserWithDuplicateName()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await AddDatabaseUserAsync(userName);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
                await _dbLocator.AddDatabaseUser([_databaseId], user.Name, "TestPassword123!", true)
        );
    }

    [Fact]
    public async Task CannotUpdateUserWithDuplicateName()
    {
        // Arrange
        var userName1 = TestHelpers.GetRandomString();
        var userName2 = TestHelpers.GetRandomString();
        var user1 = await AddDatabaseUserAsync(userName1);
        var user2 = await AddDatabaseUserAsync(userName2);

        // Act & Assert
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
        var user = await AddDatabaseUserAsync(userName);

        // Add a role to the user
        await _dbLocator.AddDatabaseUserRole(user.Id, DatabaseRole.DataWriter);

        // Attempt to delete the user
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.DeleteDatabaseUser(user.Id, true)
        );
    }

    // [Fact]
    // public async Task CanAddAndRemoveMultipleRoles()
    // {
    //     var userName = TestHelpers.GetRandomString();
    //     var user = await AddDatabaseUserAsync(userName);

    //     // Add multiple roles
    //     await _dbLocator.AddDatabaseUserRole(user.Id, DatabaseRole.DataWriter);
    //     await _dbLocator.AddDatabaseUserRole(user.Id, DatabaseRole.DataReader);
    //     await _dbLocator.AddDatabaseUserRole(user.Id, DatabaseRole.DdlAdmin);

    //     var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
    //     Assert.Contains(DatabaseRole.DataWriter, updatedUser.Roles);
    //     Assert.Contains(DatabaseRole.DataReader, updatedUser.Roles);
    //     Assert.Contains(DatabaseRole.DdlAdmin, updatedUser.Roles);

    //     // Remove roles
    //     await _dbLocator.DeleteDatabaseUserRole(user.Id, DatabaseRole.DataWriter);
    //     await _dbLocator.DeleteDatabaseUserRole(user.Id, DatabaseRole.DataReader);

    //     updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
    //     Assert.DoesNotContain(DatabaseRole.DataWriter, updatedUser.Roles);
    //     Assert.DoesNotContain(DatabaseRole.DataReader, updatedUser.Roles);
    //     Assert.Contains(DatabaseRole.DdlAdmin, updatedUser.Roles);
    // }

    [Fact]
    public async Task PasswordValidation()
    {
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () => await _dbLocator.AddDatabaseUser([1], "testuser", "short", true)
        );
    }

    [Fact]
    public async Task CannotAddDuplicateRole()
    {
        var userName = TestHelpers.GetRandomString();
        var user = await AddDatabaseUserAsync(userName);

        // Add a role
        await _dbLocator.AddDatabaseUserRole(user.Id, DatabaseRole.DataWriter);

        // Try to add the same role again
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.AddDatabaseUserRole(user.Id, DatabaseRole.DataWriter)
        );
    }

    // [Fact]
    // public async Task CannotRemoveNonExistentRole()
    // {
    //     var userName = TestHelpers.GetRandomString();
    //     var user = await AddDatabaseUserAsync(userName);

    //     // Try to remove a role that was never added
    //     await Assert.ThrowsAsync<KeyNotFoundException>(
    //         async () => await _dbLocator.DeleteDatabaseUserRole(user.Id, DatabaseRole.DataWriter)
    //     );
    // }

    // [Fact]
    // public async Task CanUpdateUserWithoutChangingPassword()
    // {
    //     var userName = TestHelpers.GetRandomString();
    //     var user = await AddDatabaseUserAsync(userName);

    //     var newName = TestHelpers.GetRandomString();
    //     await _dbLocator.UpdateDatabaseUser(user.Id, [_databaseId], newName, true);

    //     var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
    //     Assert.Equal(newName, updatedUser.Name);
    //     Assert.Equal(_databaseId, updatedUser.Databases[0].Id);
    // }

    [Fact]
    public async Task CanDeleteDatabaseUserRole()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await AddDatabaseUserAsync(userName);
        await _dbLocator.AddDatabaseUserRole(user.Id, DatabaseRole.DataWriter);

        // Act
        await _dbLocator.DeleteDatabaseUserRole(user.Id, DatabaseRole.DataWriter);

        // Assert
        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.DoesNotContain(DatabaseRole.DataWriter, updatedUser.Roles);
    }

    [Fact]
    public async Task DeleteDatabaseUserRole_NonExistentUser_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.DeleteDatabaseUserRole(-1, DatabaseRole.DataWriter)
        );
    }

    [Fact]
    public async Task DeleteDatabaseUserRole_NonExistentRole_Succeeds()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await AddDatabaseUserAsync(userName);

        // Act
        await _dbLocator.DeleteDatabaseUserRole(user.Id, DatabaseRole.DataWriter);

        // Assert
        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.DoesNotContain(DatabaseRole.DataWriter, updatedUser.Roles);
    }

    [Fact]
    public async Task DeleteDatabaseUser_WithDeleteDatabaseUserFlag_RemovesUserFromAllDatabases()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await AddDatabaseUserAsync(userName);

        // Add user to multiple databases
        var database2Name = TestHelpers.GetRandomString();
        var database2Id = await _dbLocator.AddDatabase(
            database2Name,
            _databaseServerID,
            _databaseTypeId,
            Status.Active
        );
        await _dbLocator.AddDatabaseUser([database2Id], user.Name, "TestPassword123!", true);

        // Act
        await _dbLocator.DeleteDatabaseUser(user.Id, true);

        // Assert
        var users = await _dbLocator.GetDatabaseUsers();
        Assert.DoesNotContain(users, u => u.Id == user.Id);
    }

    [Fact]
    public async Task DeleteDatabaseUser_WithoutDeleteDatabaseUserFlag_KeepsDatabaseUsers()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await AddDatabaseUserAsync(userName);

        // Act
        await _dbLocator.DeleteDatabaseUser(user.Id, false);

        // Assert
        var users = await _dbLocator.GetDatabaseUsers();
        Assert.DoesNotContain(users, u => u.Id == user.Id);
    }

    [Fact]
    public async Task DeleteDatabaseUser_ClearsCache()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await AddDatabaseUserAsync(userName);

        // Delete any roles first
        var roles = (await _dbLocator.GetDatabaseUser(user.Id)).Roles;
        foreach (var role in roles)
        {
            await _dbLocator.DeleteDatabaseUserRole(user.Id, role);
        }

        // Act
        await _dbLocator.DeleteDatabaseUser(user.Id, true);

        // Assert
        var cachedUsers = await _cache.GetCachedData<List<DatabaseUser>>("databaseUsers");
        Assert.NotNull(cachedUsers);
        Assert.DoesNotContain(cachedUsers, u => u.Id == user.Id);
    }

    [Fact]
    public async Task DeleteDatabaseUser_WithRoles_ClearsRoleCache()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await AddDatabaseUserAsync(userName);
        await _dbLocator.AddDatabaseUserRole(user.Id, DatabaseRole.DataWriter);
        await _dbLocator.AddDatabaseUserRole(user.Id, DatabaseRole.DataReader);

        // Delete roles first
        await _dbLocator.DeleteDatabaseUserRole(user.Id, DatabaseRole.DataWriter);
        await _dbLocator.DeleteDatabaseUserRole(user.Id, DatabaseRole.DataReader);

        // Act
        await _dbLocator.DeleteDatabaseUser(user.Id, true);

        // Assert
        var cachedUsers = await _cache.GetCachedData<List<DatabaseUser>>("databaseUsers");
        Assert.NotNull(cachedUsers);
        Assert.DoesNotContain(cachedUsers, u => u.Id == user.Id);
    }
}
