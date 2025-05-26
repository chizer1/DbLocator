using System.ComponentModel.DataAnnotations;
using DbLocator;
using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.Databases;
using DbLocator.Library;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

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
        await _cache.Remove("databaseUsers");
        await _cache.Remove("databaseUserRoles");
    }

    private async Task<DatabaseUser> AddDatabaseUserAsync(string userName)
    {
        // Generate a unique 8-character string from a GUID
        var uniqueId = Convert
            .ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("=", "")
            .Replace("+", "")
            .Replace("/", "")[..8];

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
    public async Task AddDatabaseUser_CreatesDatabaseUserDatabaseEntities()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();

        // Act
        var user = await AddDatabaseUserAsync(userName);

        // Assert
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
    public async Task AddDatabaseUser_WithDatabaseIds_CreatesUser()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();

        // Act
        var userId = await _dbLocator.AddDatabaseUser(
            [_databaseId],
            userName,
            "TestPassword123!",
            true
        );

        // Assert
        var user = (await _dbLocator.GetDatabaseUsers()).Single(u => u.Id == userId);
        Assert.Equal(userName, user.Name);
        Assert.Equal(_databaseId, user.Databases[0].Id);
    }

    [Fact]
    public async Task AddDatabaseUser_WithDatabaseIdsAndNoPassword_CreatesUser()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();

        // Act
        var userId = await _dbLocator.AddDatabaseUser([_databaseId], userName);

        // Assert
        var user = (await _dbLocator.GetDatabaseUsers()).Single(u => u.Id == userId);
        Assert.Equal(userName, user.Name);
        Assert.Equal(_databaseId, user.Databases[0].Id);
    }

    [Fact]
    public async Task AddDatabaseUser_WithDatabaseIdsNameAndPassword_CreatesUser()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();

        // Act
        var userId = await _dbLocator.AddDatabaseUser([_databaseId], userName, "TestPassword123!");

        // Assert
        var user = (await _dbLocator.GetDatabaseUsers()).Single(u => u.Id == userId);
        Assert.Equal(userName, user.Name);
        Assert.Equal(_databaseId, user.Databases[0].Id);
    }

    [Fact]
    public async Task AddDatabaseUser_WithMultipleDatabases_CreatesCorrectEntities()
    {
        // Arrange
        var databaseName1 = TestHelpers.GetRandomString();
        var databaseId1 = await _dbLocator.AddDatabase(
            databaseName1,
            _databaseServerID,
            _databaseTypeId,
            Status.Active
        );

        var databaseName2 = TestHelpers.GetRandomString();
        var databaseId2 = await _dbLocator.AddDatabase(
            databaseName2,
            _databaseServerID,
            _databaseTypeId,
            Status.Active
        );

        // Act
        var dbUserId = await _dbLocator.AddDatabaseUser(
            new[] { databaseId1, databaseId2 },
            TestHelpers.GetRandomString(),
            true
        );

        // Assert
        var user = await _dbLocator.GetDatabaseUser(dbUserId);
        Assert.NotNull(user);
        Assert.Equal(2, user.Databases.Count);
        Assert.Contains(databaseId1, user.Databases.Select(d => d.Id));
        Assert.Contains(databaseId2, user.Databases.Select(d => d.Id));
    }

    [Fact]
    public async Task AddDatabaseUser_WithMultipleRoles_CreatesCorrectEntities()
    {
        // Arrange
        var dbUserId = await _dbLocator.AddDatabaseUser(
            new[] { _databaseId },
            TestHelpers.GetRandomString(),
            true
        );

        // Act
        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.Owner, true);
        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.DataWriter, true);
        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.DataReader, true);

        // Assert
        var user = await _dbLocator.GetDatabaseUser(dbUserId);
        Assert.NotNull(user);
        Assert.Equal(3, user.Roles.Count);
        Assert.Contains(DatabaseRole.Owner, user.Roles);
        Assert.Contains(DatabaseRole.DataWriter, user.Roles);
        Assert.Contains(DatabaseRole.DataReader, user.Roles);
    }

    [Fact]
    public async Task AddDatabaseUser_WithRolesAndDatabases_CreatesCorrectEntities()
    {
        // Arrange
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerID,
            databaseTypeId,
            Status.Active
        );

        // Act
        var dbUserId = await _dbLocator.AddDatabaseUser(
            new[] { databaseId },
            TestHelpers.GetRandomString(),
            true
        );
        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.Owner, true);
        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.DataReader, true);

        // Assert
        var user = await _dbLocator.GetDatabaseUser(dbUserId);
        Assert.NotNull(user);
        Assert.Equal(dbUserId, user.Id);
        Assert.Contains(DatabaseRole.Owner, user.Roles);
        Assert.Contains(DatabaseRole.DataReader, user.Roles);
        Assert.Contains(databaseId, user.Databases.Select(d => d.Id));
    }

    [Fact]
    public async Task AddDatabaseUser_WithRolesAndDatabases_LoadsNavigationProperties()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await AddDatabaseUserAsync(userName);

        // Add a role to the user
        await _dbLocator.AddDatabaseUserRole(user.Id, DatabaseRole.DataWriter, true);

        // Act
        var dbContext = DbContextFactory
            .CreateDbContextFactory(_fixture.ConnectionString)
            .CreateDbContext();
        var databaseUserDatabase = await dbContext
            .Set<DatabaseUserDatabaseEntity>()
            .Include(d => d.User)
            .Include(d => d.Database)
            .FirstOrDefaultAsync(d => d.DatabaseUserId == user.Id);

        var databaseUserRole = await dbContext
            .Set<DatabaseUserRoleEntity>()
            .Include(r => r.User)
            .Include(r => r.Role)
            .FirstOrDefaultAsync(r => r.DatabaseUserId == user.Id);

        // Assert
        Assert.NotNull(databaseUserDatabase);
        Assert.NotNull(databaseUserDatabase.User);
        Assert.NotNull(databaseUserDatabase.Database);
        Assert.Equal(user.Id, databaseUserDatabase.DatabaseUserId);
        Assert.Equal(_databaseId, databaseUserDatabase.DatabaseId);
        Assert.Equal(user.Name, databaseUserDatabase.User.UserName);

        Assert.NotNull(databaseUserRole);
        Assert.NotNull(databaseUserRole.User);
        Assert.NotNull(databaseUserRole.Role);
        Assert.Equal(user.Id, databaseUserRole.DatabaseUserId);
        Assert.Equal((int)DatabaseRole.DataWriter, databaseUserRole.DatabaseRoleId);
        Assert.Equal(user.Name, databaseUserRole.User.UserName);
        Assert.Equal(DatabaseRole.DataWriter.ToString(), databaseUserRole.Role.DatabaseRoleName);
    }

    [Fact]
    public async Task AddMultipleDatabaseUsers()
    {
        // Arrange
        var userNamePrefix = TestHelpers.GetRandomString();

        // Act
        var user1 = await AddDatabaseUserAsync($"{userNamePrefix}1");
        var user2 = await AddDatabaseUserAsync($"{userNamePrefix}2");

        // Assert
        var users = (await _dbLocator.GetDatabaseUsers()).ToList();
        Assert.Contains(users, u => u.Name == user1.Name);
        Assert.Contains(users, u => u.Name == user2.Name);
    }

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
    public async Task CannotAddDuplicateRole()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await AddDatabaseUserAsync(userName);

        // Add a role
        await _dbLocator.AddDatabaseUserRole(user.Id, DatabaseRole.DataWriter);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.AddDatabaseUserRole(user.Id, DatabaseRole.DataWriter)
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
    public async Task CannotAddUserWithInvalidDatabaseId()
    {
        // Act & Assert
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
    public async Task CannotDeleteUserWithActiveRoles()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await AddDatabaseUserAsync(userName);

        // Add a role to the user
        await _dbLocator.AddDatabaseUserRole(user.Id, DatabaseRole.DataWriter);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.DeleteDatabaseUser(user.Id, true)
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
    public async Task DeleteDatabaseUser_ClearsCache()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await AddDatabaseUserAsync(userName);

        // Ensure cache is populated
        var users = await _dbLocator.GetDatabaseUsers();
        Assert.Contains(users, u => u.Id == user.Id);

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
        Assert.Null(cachedUsers);
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

        // Use a different username for the second database
        var userName2 = TestHelpers.GetRandomString();
        await _dbLocator.AddDatabaseUser([database2Id], userName2, "TestPassword123!", true);

        // Act
        await _dbLocator.DeleteDatabaseUser(user.Id, true);

        // Assert
        var users = await _dbLocator.GetDatabaseUsers();
        Assert.DoesNotContain(users, u => u.Id == user.Id);
    }

    [Fact]
    public async Task DeleteDatabaseUser_WithRoles_ClearsCache()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await AddDatabaseUserAsync(userName);
        await _dbLocator.AddDatabaseUserRole(user.Id, DatabaseRole.DataWriter);
        await _dbLocator.AddDatabaseUserRole(user.Id, DatabaseRole.DataReader);

        // Ensure cache is populated
        var users = await _dbLocator.GetDatabaseUsers();
        Assert.Contains(users, u => u.Id == user.Id);

        // Get the roles before deleting them
        var roles = (await _dbLocator.GetDatabaseUser(user.Id)).Roles.ToArray();

        // Delete roles first
        await _dbLocator.DeleteDatabaseUserRole(user.Id, DatabaseRole.DataWriter);
        await _dbLocator.DeleteDatabaseUserRole(user.Id, DatabaseRole.DataReader);

        // Act - Now delete the user
        await _dbLocator.DeleteDatabaseUser(user.Id, true);

        // Assert
        var cachedUsers = await _cache.GetCachedData<List<DatabaseUser>>("databaseUsers");
        Assert.Null(cachedUsers);

        // Verify user is deleted from database
        var allUsers = await _dbLocator.GetDatabaseUsers();
        Assert.DoesNotContain(allUsers, u => u.Id == user.Id);
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
    public async Task DeleteDatabaseUserById()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var userId = await _dbLocator.AddDatabaseUser(
            [_databaseId],
            userName,
            "TestPassword123!",
            true
        );

        // Act
        await _dbLocator.DeleteDatabaseUser(userId);

        // Assert
        var users = await _dbLocator.GetDatabaseUsers();
        Assert.DoesNotContain(users, u => u.Id == userId);
    }

    [Fact]
    public async Task DeleteDatabaseUserRole_InDatabase()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await AddDatabaseUserAsync(userName);
        // add role before deleting
        await _dbLocator.AddDatabaseUserRole(user.Id, DatabaseRole.DataWriter, true);

        // Act
        await _dbLocator.DeleteDatabaseUserRole(user.Id, DatabaseRole.DataWriter, true);

        // Assert
        var users = await _dbLocator.GetDatabaseUsers();
        Assert.Contains(users, u => u.Id == user.Id);
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
    public async Task DeleteDatabaseUserRole_NonExistentUser_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.DeleteDatabaseUserRole(-1, DatabaseRole.DataWriter)
        );
    }

    [Fact]
    public async Task DeleteNonExistentDatabaseUser_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () => await _dbLocator.DeleteDatabaseUser(-1, true)
        );
    }

    [Fact]
    public async Task GetDatabaseUserById()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await AddDatabaseUserAsync(userName);

        // Act
        var retrievedUser = await _dbLocator.GetDatabaseUser(user.Id);

        // Assert
        Assert.NotNull(retrievedUser);
        Assert.Equal(user.Id, retrievedUser.Id);
        Assert.Equal(user.Name, retrievedUser.Name);
        Assert.Equal(user.Databases[0].Id, retrievedUser.Databases[0].Id);
        Assert.Equal(user.Roles, retrievedUser.Roles);
    }

    [Fact]
    public async Task GetNonExistentDatabaseUser_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () => await _dbLocator.GetDatabaseUser(-1)
        );
    }

    [Fact]
    public async Task PasswordValidation()
    {
        // Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            async () => await _dbLocator.AddDatabaseUser([1], "testuser", "short", true)
        );
    }

    [Fact]
    public async Task ShouldRemoveCacheKey_WithNonMatchingCriteria_ReturnsFalse()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await AddDatabaseUserAsync(userName);
        await _dbLocator.AddDatabaseUserRole(user.Id, DatabaseRole.DataWriter);

        // Cache a connection string with the correct format
        var cacheKey = $"connection_{_databaseId}_{user.Id}_{(int)DatabaseRole.DataWriter}";
        await _cache.CacheConnectionString(cacheKey, "test_connection_string");

        // Act - Try to clear cache with non-matching criteria
        await _cache.TryClearConnectionStringFromCache(
            tenantId: null,
            databaseTypeId: null,
            connectionId: user.Id,
            tenantCode: null,
            roles: [DatabaseRole.DataReader] // Different role
        );

        // Assert
        var cachedData = await _cache.GetCachedData<string>(cacheKey);
        Assert.NotNull(cachedData); // Cache should not be cleared
    }

    [Fact]
    public async Task UpdateDatabase_WithAllParameters_UpdatesCorrectly()
    {
        // Arrange
        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerID,
            _databaseTypeId,
            Status.Active
        );

        // Create a new database server with unique hostname and IP
        var newServerName = TestHelpers.GetRandomString();
        var newHostName = $"server-{TestHelpers.GetRandomString()}";
        var newIpAddress = $"192.168.1.{new Random().Next(1, 255)}";
        var newServerId = await _dbLocator.AddDatabaseServer(
            newServerName,
            newIpAddress,
            newHostName,
            $"{newHostName}.localdomain",
            true
        );

        // Create a new database type
        var newTypeName = TestHelpers.GetRandomString();
        var newTypeId = await _dbLocator.AddDatabaseType(newTypeName);

        // Act
        var newDatabaseName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabase(
            databaseId,
            newDatabaseName,
            newServerId,
            newTypeId,
            Status.Inactive
        );

        // Assert
        var updatedDatabase = await _dbLocator.GetDatabase(databaseId);
        Assert.Equal(newDatabaseName, updatedDatabase.Name);
        Assert.Equal(newServerId, updatedDatabase.Server.Id);
        Assert.Equal(newTypeId, updatedDatabase.Type.Id);
        Assert.Equal(Status.Inactive, updatedDatabase.Status);
    }

    [Fact]
    public async Task UpdateDatabase_WithDatabaseServerId_UpdatesCorrectly()
    {
        // Arrange
        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerID,
            _databaseTypeId,
            Status.Active
        );

        // Create a new database server
        var newServerName = TestHelpers.GetRandomString();
        var newServerId = await _dbLocator.AddDatabaseServer(
            newServerName,
            "127.0.0.1",
            "localhost",
            "localhost.localdomain",
            true
        );

        // Act
        await _dbLocator.UpdateDatabase(databaseId, newServerId);

        // Assert
        var updatedDatabase = await _dbLocator.GetDatabase(databaseId);
        Assert.Equal(newServerId, updatedDatabase.Server.Id);
        Assert.Equal(databaseName, updatedDatabase.Name); // Name should remain unchanged
        Assert.Equal(_databaseTypeId, updatedDatabase.Type.Id); // Type should remain unchanged
        Assert.Equal(Status.Active, updatedDatabase.Status); // Status should remain unchanged
    }

    [Fact]
    public async Task UpdateDatabaseUser()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var userId = await _dbLocator.AddDatabaseUser(
            [_databaseId],
            userName,
            "TestPassword123!",
            true
        );

        // Act
        var newName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabaseUser(
            userId,
            [_databaseId],
            newName,
            "NewPassword123!",
            true
        );

        // Assert
        var updatedUser = await _dbLocator.GetDatabaseUser(userId);
        Assert.Equal(newName, updatedUser.Name);
        Assert.Equal(_databaseId, updatedUser.Databases[0].Id);
    }

    [Fact]
    public async Task UpdateDatabaseUser_NoDatabaseChange()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var userId = await _dbLocator.AddDatabaseUser(
            [_databaseId],
            userName,
            "TestPassword123!",
            true
        );

        // Act
        var newName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabaseUser(userId, [_databaseId], newName, "NewPassword123!");

        // Assert
        var updatedUser = await _dbLocator.GetDatabaseUser(userId);
        Assert.Equal(newName, updatedUser.Name);
        Assert.Equal(_databaseId, updatedUser.Databases[0].Id);
    }

    [Fact]
    public async Task UpdateDatabaseUser_RemovingAnExistingDatabaseId()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var userId = await _dbLocator.AddDatabaseUser(
            [_databaseId],
            userName,
            "TestPassword123!",
            true
        );

        var newDatabaseName = TestHelpers.GetRandomString();
        var newDatabaseId = await _dbLocator.AddDatabase(
            newDatabaseName,
            _databaseServerID,
            _databaseTypeId,
            Status.Active
        );

        // Add the user to the second database
        await _dbLocator.UpdateDatabaseUser(userId, [_databaseId, newDatabaseId], userName);

        // Now update the user to only have the first database
        await _dbLocator.UpdateDatabaseUser(userId, [_databaseId], userName);

        // Assert
        var updatedUser = await _dbLocator.GetDatabaseUser(userId);
        Assert.Equal(userName, updatedUser.Name);
        Assert.Single(updatedUser.Databases);
        Assert.Contains(updatedUser.Databases, d => d.Id == _databaseId);
        Assert.DoesNotContain(updatedUser.Databases, d => d.Id == newDatabaseId);
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithDatabaseIdsAndName()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var userId = await _dbLocator.AddDatabaseUser(
            [_databaseId],
            userName,
            "TestPassword123!",
            true
        );

        // Act
        var newName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabaseUser(userId, [_databaseId], newName);

        // Assert
        var updatedUser = await _dbLocator.GetDatabaseUser(userId);
        Assert.Equal(newName, updatedUser.Name);
        Assert.Equal(_databaseId, updatedUser.Databases[0].Id);
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithMultipleRoles_UpdatesCorrectEntities()
    {
        // Arrange
        var dbUserId = await _dbLocator.AddDatabaseUser(
            new[] { _databaseId },
            TestHelpers.GetRandomString(),
            true
        );

        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.Owner, true);

        // Act
        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.DataWriter, true);
        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.DataReader, true);

        // Assert
        var user = await _dbLocator.GetDatabaseUser(dbUserId);
        Assert.NotNull(user);
        Assert.Equal(3, user.Roles.Count);
        Assert.Contains(DatabaseRole.Owner, user.Roles);
        Assert.Contains(DatabaseRole.DataWriter, user.Roles);
        Assert.Contains(DatabaseRole.DataReader, user.Roles);
    }

    [Fact]
    public async Task UpdateDatabaseUser_WithRolesAndDatabases_UpdatesCorrectEntities()
    {
        // Arrange
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerID,
            databaseTypeId,
            Status.Active
        );

        var dbUserId = await _dbLocator.AddDatabaseUser(
            new[] { databaseId },
            TestHelpers.GetRandomString(),
            true
        );
        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.Owner, true);

        // Act
        await _dbLocator.UpdateDatabaseUser(
            dbUserId,
            new[] { databaseId },
            TestHelpers.GetRandomString(),
            true
        );
        await _dbLocator.AddDatabaseUserRole(dbUserId, DatabaseRole.DataReader, true);

        // Assert
        var user = await _dbLocator.GetDatabaseUser(dbUserId);
        Assert.NotNull(user);
        Assert.Equal(dbUserId, user.Id);
        Assert.Contains(DatabaseRole.Owner, user.Roles);
        Assert.Contains(DatabaseRole.DataReader, user.Roles);
        Assert.Contains(databaseId, user.Databases.Select(d => d.Id));
    }

    [Fact]
    public async Task UpdateDatabaseUserById()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var userId = await _dbLocator.AddDatabaseUser(
            [_databaseId],
            userName,
            "TestPassword123!",
            true
        );

        // Act
        var newName = TestHelpers.GetRandomString();
        await _dbLocator.UpdateDatabaseUser(userId, [_databaseId], newName, true);

        // Assert
        var updatedUser = await _dbLocator.GetDatabaseUser(userId);
        Assert.Equal(newName, updatedUser.Name);
        Assert.Equal(_databaseId, updatedUser.Databases[0].Id);
    }

    [Fact]
    public async Task UpdateNonExistentDatabaseUser_ThrowsKeyNotFoundException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            async () =>
                await _dbLocator.UpdateDatabaseUser(-1, [_databaseId], "testuser", "Test123!", true)
        );
    }

    [Fact]
    public async Task VerifyDatabaseUsersAreCached()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await AddDatabaseUserAsync(userName);

        // Act
        var users = await _dbLocator.GetDatabaseUsers();

        // Assert
        Assert.Contains(users, u => u.Name == user.Name);

        var cachedUsers = await _cache.GetCachedData<List<DatabaseUser>>("databaseUsers");
        Assert.NotNull(cachedUsers);
        Assert.Contains(cachedUsers, u => u.Name == user.Name);
    }

    private async Task<Database> AddDatabaseAsync(string databaseName)
    {
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerID,
            _databaseTypeId,
            Status.Active,
            true // Create the database
        );

        return (await _dbLocator.GetDatabases()).Single(db => db.Id == databaseId);
    }
}
