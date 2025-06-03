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

    public async Task<int> CreateDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus,
        bool affectDatabase = true,
        bool useTrustedConnection = false
    )
    {
        return await _createDatabase.Handle(
            new CreateDatabaseCommand(
                databaseName,
                databaseServerId,
                databaseTypeId,
                affectDatabase,
                useTrustedConnection,
                databaseStatus
            )
        );
    }

    public async Task DeleteDatabase(int databaseId, bool? deleteDatabase = true)
    {
        await _deleteDatabase.Handle(new DeleteDatabaseCommand(databaseId, deleteDatabase));
    }

    public async Task<List<Domain.Database>> GetDatabases()
    {
        var databases = await _getDatabases.Handle(new GetDatabasesQuery());

        return [.. databases];
    }

    public async Task<Domain.Database> GetDatabase(int databaseId)
    {
        return await _getDatabase.Handle(new GetDatabaseQuery(databaseId));
    }

    public async Task UpdateDatabase(
        int databaseId,
        string? databaseName,
        int? databaseServerId,
        byte? databaseTypeId,
        bool? useTrustedConnection,
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
