using DbLocator.Domain;

namespace DbLocator.Services.Database;

internal interface IDatabaseService
{
    /// <param name="databaseName">The name of the database to add.</param>
    /// <param name="databaseServerId">The ID of the database server to which the database belongs.</param>
    /// <param name="databaseTypeId">The type of the database.</param>
    /// <param name="databaseStatus">The status of the database.</param>
    /// <returns>The ID of the added database.</returns>
    Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus
    );

    /// <param name="databaseName">The name of the database to add.</param>
    /// <param name="databaseServerId">The ID of the database server to which the database belongs.</param>
    /// <param name="databaseTypeId">The type of the database.</param>
    /// <param name="databaseStatus">The status of the database.</param>
    /// <param name="affectDatabase">
    /// A flag indicating whether to perform DDL operations on the database server. If not provided, defaults to true.
    /// </param>
    /// <returns>The ID of the added database.</returns>
    Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        Status databaseStatus,
        bool affectDatabase = true
    );

    /// <param name="databaseName">The name of the database to add.</param>
    /// <param name="databaseServerId">The ID of the database server to which the database belongs.</param>
    /// <param name="databaseTypeId">The type of the database.</param>
    /// <param name="affectDatabase">
    /// A flag indicating whether to perform DDL operations on the database server. If not provided, defaults to true.
    /// </param>
    /// <returns>The ID of the added database.</returns>
    Task<int> AddDatabase(
        string databaseName,
        int databaseServerId,
        byte databaseTypeId,
        bool affectDatabase = true
    );

    /// <param name="databaseName">The name of the database to add.</param>
    /// <param name="databaseServerId">The ID of the database server to which the database belongs.</param>
    /// <param name="databaseTypeId">The type of the database.</param>
    /// <param name="databaseStatus">The status of the database.</param>
    /// <param name="affectDatabase">
    /// A flag indicating whether to perform DDL operations on the database server. If not provided, defaults to true.
    /// </param>
    /// <param name="useTrustedConnection">A flag indicating whether to use trusted connection.</param>
    /// <returns>The ID of the added database.</returns>
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
