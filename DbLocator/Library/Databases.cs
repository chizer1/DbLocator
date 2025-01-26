using DbLocator.Domain;
using DbLocator.Features.Databases;
using DbLocator.Features.Databases.AddDatabase;
using DbLocator.Features.Databases.DeleteDatabase;
using DbLocator.Features.Databases.GetDatabases;
using DbLocator.Features.Databases.UpdateDatabase;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Library;

internal class Databases
{
    private readonly AddDatabase _addDatabase;
    private readonly GetDatabases _getDatabases;
    private readonly UpdateDatabase _updateDatabase;
    private readonly DeleteDatabase _deleteDatabase;

    public Databases(DbContext dbContext)
    {
        IDatabaseRepository databaseRepository = new DatabaseRepository(dbContext);

        _addDatabase = new AddDatabase(databaseRepository);
        _getDatabases = new GetDatabases(databaseRepository);
        _updateDatabase = new UpdateDatabase(databaseRepository);
        _deleteDatabase = new DeleteDatabase(databaseRepository);
    }

    public async Task<int> AddDatabase(
        string databaseName,
        string databaseUser,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus,
        bool useTrustedConnection,
        bool createDatabase
    )
    {
        return await _addDatabase.Handle(
            new AddDatabaseCommand(
                databaseName,
                databaseUser,
                databaseServerId,
                databaseTypeId,
                databaseStatus,
                useTrustedConnection,
                createDatabase
            )
        );
    }

    public async Task<List<Database>> GetDatabases()
    {
        return await _getDatabases.Handle(new GetDatabasesQuery());
    }

    public async Task UpdateDatabase(
        int databaseId,
        string databaseName,
        string databaseUser,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus
    )
    {
        await _updateDatabase.Handle(
            new UpdateDatabaseCommand(
                databaseId,
                databaseName,
                databaseUser,
                databaseServerId,
                databaseTypeId,
                databaseStatus
            )
        );
    }

    public async Task DeleteDatabase(int databaseId)
    {
        await _deleteDatabase.Handle(new DeleteDatabaseCommand(databaseId));
    }
}
