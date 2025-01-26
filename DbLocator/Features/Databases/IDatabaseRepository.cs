using DbLocator.Domain;

namespace DbLocator.Features.Databases;

internal interface IDatabaseRepository
{
    public Task<int> AddDatabase(
        string databaseName,
        string databaseUser,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus,
        bool useTrustedConnection,
        bool createDatabase
    );
    public Task<Database> GetDatabase(int databaseId);
    public Task<List<Database>> GetDatabases();
    public Task UpdateDatabase(
        int databaseId,
        string databaseName,
        string databaseUserName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus
    );
    public Task DeleteDatabase(int databaseId);
}
