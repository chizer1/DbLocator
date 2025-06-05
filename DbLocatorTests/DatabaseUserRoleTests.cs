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
public class DatabaseUserRoleTests
{
    private readonly DbLocatorFixture _fixture;
    private readonly DbLocatorContext _dbContext;
    private readonly DbLocatorCache _cache;
    private readonly Locator _dbLocator;

    public DatabaseUserRoleTests(DbLocatorFixture fixture)
    {
        _fixture = fixture;
        _dbContext = DbContextFactory.CreateDbContextFactory(fixture.ConnectionString).CreateDbContext();
        _cache = fixture.LocatorCache;
        _dbLocator = fixture.DbLocator;
    }

    [Fact]
    public async Task DeleteDatabaseUserRole_WithAffectDatabase_RemovesRoleFromDatabase()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);
        var role = DatabaseRole.DataWriter;

        // Act
        await _dbLocator.CreateDatabaseUserRole(user.Id, role, true);
        await _dbLocator.DeleteDatabaseUserRole(user.Id, role, true);

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
    public async Task DeleteDatabaseUserRole_WithoutAffectDatabase_KeepsRoleInDatabase()
    {
        // Arrange
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);
        var role = DatabaseRole.DataWriter;

        // Act
        await _dbLocator.CreateDatabaseUserRole(user.Id, role, true);
        await _dbLocator.DeleteDatabaseUserRole(user.Id, role, false);

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
        var userName = TestHelpers.GetRandomString();
        var user = await CreateDatabaseUserAsync(userName);
        var role = DatabaseRole.DataWriter;

        // Act
        await _dbLocator.CreateDatabaseUserRole(user.Id, role, true);
        await _dbLocator.DeleteDatabaseUserRole(user.Id, role, true);

        // Assert
        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.DoesNotContain(role, updatedUser.Roles);

        var userRoles = await _dbContext
            .Set<DatabaseUserRoleEntity>()
            .Where(ur => ur.DatabaseUserId == user.Id && ur.DatabaseRoleId == (int)role)
            .ToListAsync();

        Assert.Empty(userRoles);
    }

    private async Task<DatabaseUser> CreateDatabaseUserAsync(string userName)
    {
        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _fixture.LocalhostServerId,
            await _dbLocator.CreateDatabaseType(TestHelpers.GetRandomString()),
            Status.Active
        );

        var userId = await _dbLocator.CreateDatabaseUser([databaseId], userName, "TestPassword123!", true);
        return await _dbLocator.GetDatabaseUser(userId);
    }
} 