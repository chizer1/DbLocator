using DbLocator.Db;
using DbLocator.Features.DatabaseUsers.CreateDatabaseUser;
using DbLocator.Features.DatabaseUsers.DeleteDatabaseUser;
using DbLocator.Features.DatabaseUsers.GetDatabaseUser;
using DbLocator.Features.DatabaseUsers.GetDatabaseUsers;
using DbLocator.Features.DatabaseUsers.UpdateDatabaseUser;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Services.DatabaseUser;

internal class DatabaseUserService(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    Encryption encryption,
    DbLocatorCache cache
) : IDatabaseUserService
{
    private readonly CreateDatabaseUserHandler _CreateDatabaseUser =
        new(dbContextFactory, encryption, cache);
    private readonly DeleteDatabaseUserHandler _deleteDatabaseUser = new(dbContextFactory, cache);
    private readonly GetDatabaseUserHandler _getDatabaseUser = new(dbContextFactory, cache);
    private readonly GetDatabaseUsersHandler _getDatabaseUsers = new(dbContextFactory, cache);
    private readonly UpdateDatabaseUserHandler _updateDatabaseUser =
        new(dbContextFactory, encryption, cache);

    public async Task<int> CreateDatabaseUser(
        int[] databaseIds,
        string userName,
        string userPassword,
        bool affectDatabase = true
    )
    {
        return await _CreateDatabaseUser.Handle(
            new CreateDatabaseUserCommand(userName, userPassword, databaseIds, affectDatabase)
        );
    }

    public async Task<int> CreateDatabaseUser(
        int[] databaseIds,
        string userName,
        bool affectDatabase = true
    )
    {
        var userPassword = PasswordGenerator.GenerateRandomPassword(25);

        return await _CreateDatabaseUser.Handle(
            new CreateDatabaseUserCommand(userName, userPassword, databaseIds, affectDatabase)
        );
    }

    public async Task<int> CreateDatabaseUser(
        int[] databaseIds,
        string userName,
        string userPassword
    )
    {
        return await _CreateDatabaseUser.Handle(
            new CreateDatabaseUserCommand(userName, userPassword, databaseIds, true)
        );
    }

    public async Task<int> CreateDatabaseUser(int[] databaseIds, string userName)
    {
        var userPassword = PasswordGenerator.GenerateRandomPassword(25);

        return await _CreateDatabaseUser.Handle(
            new CreateDatabaseUserCommand(userName, userPassword, databaseIds, true)
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
        return await _getDatabaseUser.Handle(new GetDatabaseUserQuery(databaseUserId));
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
        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                databaseUserId,
                [.. databaseIds],
                databaseUserName,
                null,
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
                [.. databaseIds],
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
        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                databaseUserId,
                [.. databaseIds],
                databaseUserName,
                null,
                true
            )
        );
    }
}
