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
        IEnumerable<DatabaseRole> UserRoles,
        bool CreateUser
    )
    {
        return await _addDatabaseUser.Handle(
            new AddDatabaseUserCommand(DatabaseId, UserName, UserPassword, UserRoles, CreateUser)
        );
    }

    // Allow library users to provide a factory for generating user passwords
    internal async Task<int> AddDatabaseUser(
        int DatabaseId,
        string UserName,
        Func<string> UserPasswordFactory,
        IEnumerable<DatabaseRole> UserRoles,
        bool CreateUser
    )
    {
        var UserPassword = UserPasswordFactory();

        return await _addDatabaseUser.Handle(
            new AddDatabaseUserCommand(DatabaseId, UserName, UserPassword, UserRoles, CreateUser)
        );
    }

    internal async Task<int> AddDatabaseUser(
        int DatabaseId,
        string UserName,
        IEnumerable<DatabaseRole> UserRoles,
        bool CreateUser
    )
    {
        var UserPassword = PasswordGenerator.GenerateRandomPassword(25);
        return await _addDatabaseUser.Handle(
            new AddDatabaseUserCommand(DatabaseId, UserName, UserPassword, UserRoles, CreateUser)
        );
    }

    internal async Task<int> AddDatabaseUser(
        int DatabaseId,
        string UserName,
        string UserPassword,
        IEnumerable<DatabaseRole> UserRoles
    )
    {
        return await _addDatabaseUser.Handle(
            new AddDatabaseUserCommand(DatabaseId, UserName, UserPassword, UserRoles, false)
        );
    }

    // Allow library users to provide a factory for generating user passwords
    internal async Task<int> AddDatabaseUser(
        int DatabaseId,
        string UserName,
        Func<string> UserPasswordFactory,
        IEnumerable<DatabaseRole> UserRoles
    )
    {
        var UserPassword = UserPasswordFactory();

        return await _addDatabaseUser.Handle(
            new AddDatabaseUserCommand(DatabaseId, UserName, UserPassword, UserRoles, false)
        );
    }

    internal async Task<int> AddDatabaseUser(
        int DatabaseId,
        string UserName,
        IEnumerable<DatabaseRole> UserRoles
    )
    {
        var UserPassword = PasswordGenerator.GenerateRandomPassword(25);
        return await _addDatabaseUser.Handle(
            new AddDatabaseUserCommand(DatabaseId, UserName, UserPassword, UserRoles, false)
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
        IEnumerable<DatabaseRole> UserRoles,
        bool UpdateDatabase
    )
    {
        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                DatabaseUserId,
                DatabaseUserName,
                DatabaseUserPassword,
                UserRoles,
                UpdateDatabase
            )
        );
    }

    internal async Task UpdateDatabaseUser(
        int DatabaseUserId,
        string DatabaseUserName,
        Func<string> DatabaseUserPasswordFactory,
        IEnumerable<DatabaseRole> UserRoles,
        bool UpdateDatabase
    )
    {
        var DatabaseUserPassword = DatabaseUserPasswordFactory();
        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                DatabaseUserId,
                DatabaseUserName,
                DatabaseUserPassword,
                UserRoles,
                UpdateDatabase
            )
        );
    }

    internal async Task UpdateDatabaseUser(
        int DatabaseUserId,
        string DatabaseUserName,
        IEnumerable<DatabaseRole> UserRoles,
        bool UpdateDatabase
    )
    {
        var DatabaseUserPassword = PasswordGenerator.GenerateRandomPassword(25);
        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                DatabaseUserId,
                DatabaseUserName,
                DatabaseUserPassword,
                UserRoles,
                UpdateDatabase
            )
        );
    }

    internal async Task UpdateDatabaseUser(
        int DatabaseUserId,
        string DatabaseUserName,
        string DatabaseUserPassword,
        IEnumerable<DatabaseRole> UserRoles
    )
    {
        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                DatabaseUserId,
                DatabaseUserName,
                DatabaseUserPassword,
                UserRoles,
                false
            )
        );
    }

    internal async Task UpdateDatabaseUser(
        int DatabaseUserId,
        string DatabaseUserName,
        Func<string> DatabaseUserPasswordFactory,
        IEnumerable<DatabaseRole> UserRoles
    )
    {
        var DatabaseUserPassword = DatabaseUserPasswordFactory();
        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                DatabaseUserId,
                DatabaseUserName,
                DatabaseUserPassword,
                UserRoles,
                false
            )
        );
    }

    internal async Task UpdateDatabaseUser(
        int DatabaseUserId,
        string DatabaseUserName,
        IEnumerable<DatabaseRole> UserRoles
    )
    {
        var DatabaseUserPassword = PasswordGenerator.GenerateRandomPassword(25);
        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                DatabaseUserId,
                DatabaseUserName,
                DatabaseUserPassword,
                UserRoles,
                false
            )
        );
    }
}
