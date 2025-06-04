#nullable enable

namespace DbLocator.Services.DatabaseUser;

internal interface IDatabaseUserService
{
    Task<int> CreateDatabaseUser(
        int[] databaseIds,
        string userName,
        string userPassword,
        bool affectDatabase
    );
    Task DeleteDatabaseUser(int databaseUserId, bool? affectDatabase);
    Task<List<Domain.DatabaseUser>> GetDatabaseUsers();
    Task<Domain.DatabaseUser> GetDatabaseUser(int databaseUserId);
    Task UpdateDatabaseUser(
        int databaseUserId,
        string? userName,
        string? userPassword,
        int[]? databaseIds,
        bool? affectDatabase
    );
}
