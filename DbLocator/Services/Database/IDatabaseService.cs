using DbLocator.Domain;

namespace DbLocator.Services.Database;

internal interface IDatabaseService
{
    Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus
    );
    Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus,
        bool affectDatabase = true
    );
    Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        bool affectDatabase = true
    );
    Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus,
        bool affectDatabase = true,
        bool useTrustedConnection = false
    );
    Task DeleteDatabase(int databaseId);
    Task DeleteDatabase(int databaseId, bool deleteDatabase);
    Task<List<Domain.Database>> GetDatabases();
    Task<Domain.Database> GetDatabase(int databaseId);
    Task UpdateDatabase(
        int databaseId,
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus
    );
    Task UpdateDatabase(int databaseId, int databaseServerId);
    Task UpdateDatabase(int databaseId, byte databaseTypeId);
    Task UpdateDatabase(int databaseId, string databaseName);
    Task UpdateDatabase(int databaseId, Status databaseStatus);
    Task UpdateDatabase(int databaseId, bool useTrustedConnection);
}
