#nullable enable

using DbLocator.Db;
using DbLocator.Features.DatabaseUsers.CreateDatabaseUser;
using DbLocator.Features.DatabaseUsers.DeleteDatabaseUser;
using DbLocator.Features.DatabaseUsers.GetDatabaseUserById;
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
    private readonly GetDatabaseUserByIdHandler _getDatabaseUserById = new(dbContextFactory, cache);
    private readonly GetDatabaseUsersHandler _getDatabaseUsers = new(dbContextFactory, cache);
    private readonly UpdateDatabaseUserHandler _updateDatabaseUser =
        new(dbContextFactory, encryption, cache);

    public async Task<int> CreateDatabaseUser(
        int[] databaseIds,
        string userName,
        string userPassword,
        bool affectDatabase
    )
    {
        var password = userPassword ?? PasswordGenerator.GenerateRandomPassword(25);

        return await _CreateDatabaseUser.Handle(
            new CreateDatabaseUserCommand(userName, password, databaseIds, affectDatabase)
        );
    }

    public async Task DeleteDatabaseUser(int databaseUserId, bool? affectDatabase)
    {
        await _deleteDatabaseUser.Handle(
            new DeleteDatabaseUserCommand(databaseUserId, affectDatabase ?? true)
        );
    }

    public async Task<List<Domain.DatabaseUser>> GetDatabaseUsers()
    {
        return await _getDatabaseUsers.Handle(new GetDatabaseUsersQuery());
    }

    public async Task<Domain.DatabaseUser> GetDatabaseUser(int databaseUserId)
    {
        return await _getDatabaseUserById.Handle(new GetDatabaseUserByIdQuery(databaseUserId));
    }

    public async Task UpdateDatabaseUser(
        int databaseUserId,
        string? userName,
        string? userPassword,
        int[]? databaseIds,
        bool? affectDatabase
    )
    {
        await _updateDatabaseUser.Handle(
            new UpdateDatabaseUserCommand(
                databaseUserId,
                databaseIds ?? [],
                userName ?? throw new ArgumentNullException(nameof(userName)),
                userPassword,
                affectDatabase ?? true
            )
        );
    }
}
