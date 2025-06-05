using DbLocator.Domain;
using DbLocator.Features.DatabaseUserRoles.DeleteDatabaseUserRole;
using DbLocator.Features.DatabaseUserRoles.CreateDatabaseUserRole;
using DbLocator.Db;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace DbLocatorTests;

public class DatabaseUserRoleTests : IClassFixture<TestFixture>
{
    private readonly TestFixture _fixture;
    private readonly DbLocatorContext _dbContext;
    private readonly DbLocatorCache _cache;

    public DatabaseUserRoleTests(TestFixture fixture)
    {
        _fixture = fixture;
        _dbContext = fixture.DbContext;
        _cache = fixture.Cache;
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

        // Verify role still exists in database
        var databases = await _dbContext
            .Set<DatabaseUserDatabaseEntity>()
            .Include(dud => dud.Database)
            .Include(dud => dud.Database.DatabaseServer)
            .Where(dud => dud.DatabaseUserId == user.Id)
            .Select(dud => dud.Database)
            .ToListAsync();

        foreach (var database in databases)
        {
            var dbName = Sql.SanitizeSqlIdentifier(database.DatabaseName);
            var userName = Sql.SanitizeSqlIdentifier(user.UserName);
            var roleName = Sql.SanitizeSqlIdentifier($"db_{role.ToString().ToLower()}");

            var result = await Sql.ExecuteSqlQueryAsync(
                _dbContext,
                $"use [{dbName}]; SELECT 1 FROM sys.database_role_members WHERE role_principal_id = DATABASE_PRINCIPAL_ID('{roleName}') AND member_principal_id = DATABASE_PRINCIPAL_ID('{userName}');",
                database.DatabaseServer.IsLinkedServer,
                database.DatabaseServer.DatabaseServerHostName
            );

            Assert.True(result.Any(), $"Role {role} should still exist in database {dbName} for user {userName}");
        }
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
        await _dbLocator.DeleteDatabaseUserRole(user.Id, role, null);

        // Assert
        var updatedUser = await _dbLocator.GetDatabaseUser(user.Id);
        Assert.DoesNotContain(role, updatedUser.Roles);

        // Verify role is removed from database
        var databases = await _dbContext
            .Set<DatabaseUserDatabaseEntity>()
            .Include(dud => dud.Database)
            .Include(dud => dud.Database.DatabaseServer)
            .Where(dud => dud.DatabaseUserId == user.Id)
            .Select(dud => dud.Database)
            .ToListAsync();

        foreach (var database in databases)
        {
            var dbName = Sql.SanitizeSqlIdentifier(database.DatabaseName);
            var userName = Sql.SanitizeSqlIdentifier(user.UserName);
            var roleName = Sql.SanitizeSqlIdentifier($"db_{role.ToString().ToLower()}");

            var result = await Sql.ExecuteSqlQueryAsync(
                _dbContext,
                $"use [{dbName}]; SELECT 1 FROM sys.database_role_members WHERE role_principal_id = DATABASE_PRINCIPAL_ID('{roleName}') AND member_principal_id = DATABASE_PRINCIPAL_ID('{userName}');",
                database.DatabaseServer.IsLinkedServer,
                database.DatabaseServer.DatabaseServerHostName
            );

            Assert.False(result.Any(), $"Role {role} should be removed from database {dbName} for user {userName}");
        }
    }

    private async Task<DatabaseUser> CreateDatabaseUserAsync(string userName)
    {
        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.CreateDatabase(
            databaseName,
            _fixture.DatabaseServerId,
            _fixture.DatabaseTypeId,
            Status.Active
        );

        return await _dbLocator.CreateDatabaseUser([databaseId], userName, "TestPassword123!", true);
    }
} 