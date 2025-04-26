using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Features.Databases;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Library;

internal class Databases(IDbContextFactory<DbLocatorContext> dbContextFactory, DbLocatorCache cache)
{
    private readonly AddDatabase _addDatabase = new(dbContextFactory, cache);
    private readonly DeleteDatabase _deleteDatabase = new(dbContextFactory, cache);
    private readonly GetDatabase _getDatabase = new(dbContextFactory);
    private readonly GetDatabases _getDatabases = new(dbContextFactory, cache);
    private readonly UpdateDatabase _updateDatabase = new(dbContextFactory, cache);

    internal async Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus
    )
    {
        return await _addDatabase.Handle(
            new AddDatabaseCommand(
                databaseName,
                databaseServerId,
                databaseTypeId,
                databaseStatus,
                false,
                true
            )
        );
    }

    internal async Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus,
        bool createDatabase
    )
    {
        return await _addDatabase.Handle(
            new AddDatabaseCommand(
                databaseName,
                databaseServerId,
                databaseTypeId,
                databaseStatus,
                false,
                createDatabase
            )
        );
    }

    internal async Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        bool createDatabase
    )
    {
        return await _addDatabase.Handle(
            new AddDatabaseCommand(
                databaseName,
                databaseServerId,
                databaseTypeId,
                Status.Active,
                true,
                createDatabase
            )
        );
    }

    internal async Task DeleteDatabase(int databaseId)
    {
        await _deleteDatabase.Handle(new DeleteDatabaseCommand(databaseId, false));
    }

    internal async Task DeleteDatabase(int databaseId, bool deleteDatabase)
    {
        await _deleteDatabase.Handle(new DeleteDatabaseCommand(databaseId, deleteDatabase));
    }

    internal async Task<List<Database>> GetDatabases()
    {
        return await _getDatabases.Handle(new GetDatabasesQuery());
    }

    internal async Task<Database> GetDatabase(int databaseId)
    {
        return await _getDatabase.Handle(new GetDatabaseQuery(databaseId));
    }

    internal async Task UpdateDatabase(
        int databaseId,
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus
    )
    {
        await _updateDatabase.Handle(
            new UpdateDatabaseCommand(
                databaseId,
                databaseName,
                databaseServerId,
                databaseTypeId,
                databaseStatus,
                null
            )
        );
    }

    internal async Task UpdateDatabase(int databaseId, int databaseServerId)
    {
        await _updateDatabase.Handle(
            new UpdateDatabaseCommand(databaseId, null, databaseServerId, null, null, null)
        );
    }

    internal async Task UpdateDatabase(int databaseId, byte databaseTypeId)
    {
        await _updateDatabase.Handle(
            new UpdateDatabaseCommand(databaseId, null, null, databaseTypeId, null, null)
        );
    }

    internal async Task UpdateDatabase(int databaseId, string databaseName)
    {
        await _updateDatabase.Handle(
            new UpdateDatabaseCommand(databaseId, databaseName, null, null, null, null)
        );
    }

    internal async Task UpdateDatabase(int databaseId, Status databaseStatus)
    {
        await _updateDatabase.Handle(
            new UpdateDatabaseCommand(databaseId, null, null, null, databaseStatus, null)
        );
    }

    internal async Task UpdateDatabase(int databaseId, bool useTrustedConnection)
    {
        await _updateDatabase.Handle(
            new UpdateDatabaseCommand(databaseId, null, null, null, null, useTrustedConnection)
        );
    }
}
