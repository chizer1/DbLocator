using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.DatabaseUsers;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Library;

internal class DatabaseUsers(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    Encryption encryption
)
{
    private readonly AddDatabaseUser _addDatabaseUser = new(dbContextFactory, encryption);
    private readonly DeleteDatabaseUser _deleteDatabaseUser = new(dbContextFactory);
    private readonly GetDatabaseUsers _getDatabaseUsers = new(dbContextFactory);
    private readonly UpdateDatabaseUser _updateDatabaseUser = new(dbContextFactory, encryption);

    internal async Task<int> AddDatabaseUser(
        int DatabaseId,
        string UserName,
        string UserPassword,
        bool CreateUser
    )
    {
        return await _addDatabaseUser.Handle(
            new AddDatabaseUserCommand(DatabaseId, UserName, UserPassword, CreateUser)
        );
    }

    internal async Task<int> AddDatabaseUser(int DatabaseId, string UserName, bool CreateUser)
    {
        var UserPassword = PasswordGenerator.GenerateRandomPassword(25);
        return await _addDatabaseUser.Handle(
            new AddDatabaseUserCommand(DatabaseId, UserName, UserPassword, CreateUser)
        );
    }

    internal async Task<int> AddDatabaseUser(int DatabaseId, string UserName, string UserPassword)
    {
        return await _addDatabaseUser.Handle(
            new AddDatabaseUserCommand(DatabaseId, UserName, UserPassword, false)
        );
    }

    internal async Task<int> AddDatabaseUser(int DatabaseId, string UserName)
    {
        var UserPassword = PasswordGenerator.GenerateRandomPassword(25);
        return await _addDatabaseUser.Handle(
            new AddDatabaseUserCommand(DatabaseId, UserName, UserPassword, false)
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
