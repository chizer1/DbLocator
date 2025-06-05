using DbLocator;
using DbLocator.Domain;
using DbLocator.Features.DatabaseUserRoles.DeleteDatabaseUserRole;
using DbLocator.Features.DatabaseUserRoles.CreateDatabaseUserRole;
using DbLocator.Db;
using DbLocator.Utilities;
using DbLocatorTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class DatabaseUserRoleTests : IAsyncLifetime
{
    private readonly DbLocatorFixture _fixture;
    private readonly DbLocatorContext _dbContext;
    private readonly DbLocatorCache _cache;
    private readonly Locator _dbLocator;
    private readonly List<DatabaseUser> _testUsers = new();

    public DatabaseUserRoleTests(DbLocatorFixture fixture)
    {
        _fixture = fixture;
        _dbContext = DbContextFactory.CreateDbContextFactory(fixture.ConnectionString).CreateDbContext();
        _cache = fixture.LocatorCache;
        _dbLocator = fixture.DbLocator;
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
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

    private async Task<DatabaseUser> CreateDatabaseUserAsync(string userName = null)
    {
        userName ??= TestHelpers.GetRandomString();
        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _fixture.LocalhostServerId,
            await _dbLocator.CreateDatabaseType(TestHelpers.GetRandomString()),
            Status.Active
        );

        var userId = await _dbLocator.CreateDatabaseUser([databaseId], userName, "TestPassword123!", true);
        var user = await _dbLocator.GetDatabaseUser(userId);
        _testUsers.Add(user);
        return user;
    }

    #region Delete Tests
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task DeleteDatabaseUserRole_WithAffectDatabaseFlag_HandlesCorrectly(bool affectDatabase)
    {
        // Arrange
        var user = await CreateDatabaseUserAsync();
        var role = DatabaseRole.DataWriter;

        // Act
        await _dbLocator.CreateDatabaseUserRole(user.Id, role, true);
        await _dbLocator.DeleteDatabaseUserRole(user.Id, role, affectDatabase);

        // Assert
        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.DoesNotContain(role, updatedUser.Roles);

        var userRoles = await _dbContext
            .Set<DatabaseUserRoleEntity>()
            .Where(ur => ur.DatabaseUserId == user.Id && ur.DatabaseRoleId == (int)role)
            .ToListAsync();

        Assert.Empty(userRoles);
    }

    [Fact]
    public async Task DeleteDatabaseUserRole_WithNullAffectDatabase_DefaultsToTrue()
    {
        // Arrange
        var user = await CreateDatabaseUserAsync();
        var role = DatabaseRole.DataWriter;

        // Act
        await _dbLocator.CreateDatabaseUserRole(user.Id, role, true);
        await _dbLocator.DeleteDatabaseUserRole(user.Id, role);

        // Assert
        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.DoesNotContain(role, updatedUser.Roles);

        var userRoles = await _dbContext
            .Set<DatabaseUserRoleEntity>()
            .Where(ur => ur.DatabaseUserId == user.Id && ur.DatabaseRoleId == (int)role)
            .ToListAsync();

        Assert.Empty(userRoles);
    }

    [Fact]
    public async Task DeleteDatabaseUserRole_UserDoesNotHaveRole_ThrowsInvalidOperationException()
    {
        // Arrange
        var user = await CreateDatabaseUserAsync();
        var role = DatabaseRole.DataWriter;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _dbLocator.DeleteDatabaseUserRole(user.Id, role)
        );
        
        Assert.Equal($"User '{user.Name}' does not have role '{role}'.", exception.Message);
    }
    #endregion
} 