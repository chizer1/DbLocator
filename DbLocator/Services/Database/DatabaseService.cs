#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.Databases.CreateDatabase;
using DbLocator.Features.Databases.DeleteDatabase;
using DbLocator.Features.Databases.GetDatabase;
using DbLocator.Features.Databases.GetDatabases;
using DbLocator.Features.Databases.UpdateDatabase;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Services.Database;

internal class DatabaseService(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
) : IDatabaseService
{
    private readonly CreateDatabaseHandler _createDatabase = new(dbContextFactory, cache);
    private readonly DeleteDatabaseHandler _deleteDatabase = new(dbContextFactory, cache);
    private readonly GetDatabaseHandler _getDatabase = new(dbContextFactory, cache);
    private readonly GetDatabasesHandler _getDatabases = new(dbContextFactory, cache);
    private readonly UpdateDatabaseHandler _updateDatabase = new(dbContextFactory, cache);

    public Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus
    ) => AddDatabase(databaseName, databaseServerId, databaseTypeId, databaseStatus, true, false);

    public Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus,
        bool affectDatabase = true
    ) =>
        AddDatabase(
            databaseName,
            databaseServerId,
            databaseTypeId,
            databaseStatus,
            affectDatabase,
            false
        );

    public Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        bool affectDatabase = true
    ) =>
        AddDatabase(
            databaseName,
            databaseServerId,
            databaseTypeId,
            Status.Active,
            affectDatabase,
            false
        );

    public async Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus,
        bool affectDatabase = true,
        bool useTrustedConnection = false
    )
    {
        var database = await _createDatabase.Handle(
            new CreateDatabaseCommand(
                databaseName,
                databaseServerId,
                databaseTypeId,
                affectDatabase,
                useTrustedConnection,
                databaseStatus
            )
        );
        return database.Id;
    }

    public Task DeleteDatabase(int databaseId) => DeleteDatabase(databaseId, true);

    public async Task DeleteDatabase(int databaseId, bool deleteDatabase)
    {
        await _deleteDatabase.Handle(new DeleteDatabaseCommand(databaseId));
    }

    public async Task<List<Domain.Database>> GetDatabases()
    {
        var databases = await _getDatabases.Handle(new GetDatabasesQuery());
        return databases.ToList();
    }

    public async Task<Domain.Database> GetDatabase(int databaseId)
    {
        return await _getDatabase.Handle(new GetDatabaseQuery(databaseId));
    }

    public Task UpdateDatabase(
        int databaseId,
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus
    ) =>
        UpdateDatabase(
            databaseId,
            databaseName,
            databaseServerId,
            databaseTypeId,
            false,
            databaseStatus
        );

    public Task UpdateDatabase(int databaseId, int databaseServerId) =>
        UpdateDatabase(databaseId, string.Empty, databaseServerId, 0, false);

    public Task UpdateDatabase(int databaseId, byte databaseTypeId) =>
        UpdateDatabase(databaseId, string.Empty, 0, databaseTypeId, false);

    public Task UpdateDatabase(int databaseId, string databaseName) =>
        UpdateDatabase(databaseId, databaseName, 0, 0, false);

    public Task UpdateDatabase(int databaseId, Status databaseStatus) =>
        UpdateDatabase(databaseId, string.Empty, 0, 0, false, databaseStatus);

    public Task UpdateDatabase(int databaseId, bool useTrustedConnection) =>
        UpdateDatabase(databaseId, string.Empty, 0, 0, useTrustedConnection);

    private async Task UpdateDatabase(
        int databaseId,
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        bool useTrustedConnection,
        Status? status = null
    )
    {
        await _updateDatabase.Handle(
            new UpdateDatabaseCommand(
                databaseId,
                databaseName,
                databaseServerId,
                databaseTypeId,
                useTrustedConnection,
                status ?? Status.Active
            )
        );
    }
}
