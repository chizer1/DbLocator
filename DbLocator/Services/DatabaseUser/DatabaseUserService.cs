using DbLocator.Db;
using DbLocator.Features.DatabaseUsers;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Services.DatabaseUser;

internal class DatabaseUserService(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    Encryption encryption,
    DbLocatorCache cache
) : IDatabaseUserService
{
    private readonly AddDatabaseUser _addDatabaseUser = new(dbContextFactory, encryption, cache);
    private readonly DeleteDatabaseUser _deleteDatabaseUser = new(dbContextFactory, cache);
    private readonly GetDatabaseUser _getDatabaseUser = new(dbContextFactory, cache);
    private readonly GetDatabaseUsers _getDatabaseUsers = new(dbContextFactory, cache);
    private readonly UpdateDatabaseUser _updateDatabaseUser =
        new(dbContextFactory, encryption, cache);

    public async Task<int> AddDatabaseUser(
        int[] databaseIds,
        string userName,
        string userPassword,
        bool affectDatabase
    )
    {
        return await _addDatabaseUser.Handle(
            new AddDatabaseUserCommand(userName, userPassword, databaseIds, affectDatabase)
        );
    }

    public async Task<int> AddDatabaseUser(int[] databaseIds, string userName, bool affectDatabase)
    {
        var userPassword = PasswordGenerator.GenerateRandomPassword(25);

        return await _addDatabaseUser.Handle(
            new AddDatabaseUserCommand(userName, userPassword, databaseIds, affectDatabase)
        );
    }

    public async Task<int> AddDatabaseUser(int[] databaseIds, string userName, string userPassword)
    {
        return await _addDatabaseUser.Handle(
            new AddDatabaseUserCommand(userName, userPassword, databaseIds, true)
        );
    }

    public async Task<int> AddDatabaseUser(int[] databaseIds, string userName)
    {
        var userPassword = PasswordGenerator.GenerateRandomPassword(25);

        return await _addDatabaseUser.Handle(
            new AddDatabaseUserCommand(userName, userPassword, databaseIds, true)
        );
    }

    public async Task DeleteDatabaseUser(int databaseUserId)
    {
        await _deleteDatabaseUser.Handle(new DeleteDatabaseUserCommand(databaseUserId, true));
    }

    public async Task DeleteDatabaseUser(int databaseUserId, bool deleteDatabaseUser)
    {
        await _deleteDatabaseUser.Handle(
            new DeleteDatabaseUserCommand(databaseUserId, deleteDatabaseUser)
        );
    }

    public async Task<List<Domain.DatabaseUser>> GetDatabaseUsers()
    {
        return await _getDatabaseUsers.Handle(new GetDatabaseUsersQuery());
    }

    public async Task<Domain.DatabaseUser> GetDatabaseUser(int databaseUserId)
    {
        return await _getDatabaseUser.Handle(
            new GetDatabaseUserQuery { DatabaseUserId = databaseUserId }
        );
    }

    public async Task UpdateDatabaseUser(
        int databaseUserId,
        int[] databaseIds,
        string databaseUserName,
        string databaseUserPassword,
        bool updateDatabase
    )
    {
        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                databaseUserId,
                [.. databaseIds],
                databaseUserName,
                databaseUserPassword,
                updateDatabase
            )
        );
    }

    public async Task UpdateDatabaseUser(
        int databaseUserId,
        int[] databaseIds,
        string databaseUserName,
        bool updateDatabase
    )
    {
        var databaseUserPassword = PasswordGenerator.GenerateRandomPassword(25);

        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                databaseUserId,
                databaseIds.ToList(),
                databaseUserName,
                databaseUserPassword,
                updateDatabase
            )
        );
    }

    public async Task UpdateDatabaseUser(
        int databaseUserId,
        int[] databaseIds,
        string databaseUserName,
        string databaseUserPassword
    )
    {
        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                databaseUserId,
                databaseIds.ToList(),
                databaseUserName,
                databaseUserPassword,
                true
            )
        );
    }

    public async Task UpdateDatabaseUser(
        int databaseUserId,
        int[] databaseIds,
        string databaseUserName
    )
    {
        var databaseUserPassword = PasswordGenerator.GenerateRandomPassword(25);

        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                databaseUserId,
                databaseIds.ToList(),
                databaseUserName,
                databaseUserPassword,
                true
            )
        );
    }

    public async Task UpdateDatabaseUser(
        int databaseUserId,
        string databaseUserName,
        string databaseUserPassword
    )
    {
        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                databaseUserId,
                null,
                databaseUserName,
                databaseUserPassword,
                true
            )
        );
    }

    public async Task UpdateDatabaseUser(int databaseUserId, string databaseUserName)
    {
        var databaseUserPassword = PasswordGenerator.GenerateRandomPassword(25);

        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                databaseUserId,
                null,
                databaseUserName,
                databaseUserPassword,
                true
            )
        );
    }

    public async Task UpdateDatabaseUser(int databaseUserId, int[] databaseIds)
    {
        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(databaseUserId, databaseIds.ToList(), null, null, true)
        );
    }
}
