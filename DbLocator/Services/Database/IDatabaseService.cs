#nullable enable

using DbLocator.Domain;

namespace DbLocator.Services.Database;

internal interface IDatabaseService
{
    Task<int> CreateDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus,
        bool affectDatabase = true,
        bool useTrustedConnection = false
    );
    Task DeleteDatabase(int databaseId, bool? affectDatabase);
    Task<List<Domain.Database>> GetDatabases();
    Task<Domain.Database> GetDatabase(int databaseId);
    Task UpdateDatabase(
        int databaseId,
        string? databaseName,
        int? databaseServerId,
        byte? databaseTypeId,
        bool? useTrustedConnection,
        Status? status,
        bool? affectDatabase
    );
}
