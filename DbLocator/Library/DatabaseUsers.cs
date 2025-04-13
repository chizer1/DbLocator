using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.DatabaseUsers;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Library;

internal class DatabaseUsers(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    Encryption encryption,
    DbLocatorCache cache
)
{
    private readonly AddDatabaseUser _addDatabaseUser = new(dbContextFactory, encryption, cache);
    private readonly DeleteDatabaseUser _deleteDatabaseUser = new(dbContextFactory, cache);
    private readonly GetDatabaseUsers _getDatabaseUsers = new(dbContextFactory, cache);
    private readonly UpdateDatabaseUser _updateDatabaseUser =
        new(dbContextFactory, encryption, cache);

    internal async Task<int> AddDatabaseUser(
        int databaseId,
        string userName,
        string userPassword,
        bool createUser
    )
    {
        return await _addDatabaseUser.Handle(
            new AddDatabaseUserCommand(databaseId, userName, userPassword, createUser)
        );
    }

    internal async Task<int> AddDatabaseUser(int databaseId, string userName, bool createUser)
    {
        var userPassword = PasswordGenerator.GenerateRandomPassword(25);
        return await _addDatabaseUser.Handle(
            new AddDatabaseUserCommand(databaseId, userName, userPassword, createUser)
        );
    }

    internal async Task<int> AddDatabaseUser(int databaseId, string userName, string userPassword)
    {
        return await _addDatabaseUser.Handle(
            new AddDatabaseUserCommand(databaseId, userName, userPassword, false)
        );
    }

    internal async Task<int> AddDatabaseUser(int databaseId, string userName)
    {
        var userPassword = PasswordGenerator.GenerateRandomPassword(25);
        return await _addDatabaseUser.Handle(
            new AddDatabaseUserCommand(databaseId, userName, userPassword, false)
        );
    }

    internal async Task DeleteDatabaseUser(int databaseUserId)
    {
        await _deleteDatabaseUser.Handle(new DeleteDatabaseUserCommand(databaseUserId, false));
    }

    internal async Task DeleteDatabaseUser(int databaseUserId, bool deleteDatabaseUser)
    {
        await _deleteDatabaseUser.Handle(
            new DeleteDatabaseUserCommand(databaseUserId, deleteDatabaseUser)
        );
    }

    internal async Task<List<DatabaseUser>> GetDatabaseUsers()
    {
        return await _getDatabaseUsers.Handle(new GetDatabaseUsersQuery());
    }

    internal async Task UpdateDatabaseUser(
        int DatabaseUserId,
        string DatabaseUserName,
        string DatabaseUserPassword,
        bool UpdateDatabase
    )
    {
        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                DatabaseUserId,
                DatabaseUserName,
                DatabaseUserPassword,
                UpdateDatabase
            )
        );
    }

    internal async Task UpdateDatabaseUser(
        int DatabaseUserId,
        string DatabaseUserName,
        bool UpdateDatabase
    )
    {
        var DatabaseUserPassword = PasswordGenerator.GenerateRandomPassword(25);
        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                DatabaseUserId,
                DatabaseUserName,
                DatabaseUserPassword,
                UpdateDatabase
            )
        );
    }

    internal async Task UpdateDatabaseUser(
        int DatabaseUserId,
        string DatabaseUserName,
        string DatabaseUserPassword
    )
    {
        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                DatabaseUserId,
                DatabaseUserName,
                DatabaseUserPassword,
                false
            )
        );
    }

    internal async Task UpdateDatabaseUser(int DatabaseUserId, string DatabaseUserName)
    {
        var DatabaseUserPassword = PasswordGenerator.GenerateRandomPassword(25);
        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                DatabaseUserId,
                DatabaseUserName,
                DatabaseUserPassword,
                false
            )
        );
    }
}
