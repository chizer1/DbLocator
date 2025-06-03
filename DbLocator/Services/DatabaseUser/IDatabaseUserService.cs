namespace DbLocator.Services.DatabaseUser;

internal interface IDatabaseUserService
{
    Task<int> CreateDatabaseUser(
        int[] databaseIds,
        string userName,
        string userPassword,
        bool affectDatabase = true
    );
    Task<int> CreateDatabaseUser(int[] databaseIds, string userName, bool affectDatabase = true);
    Task<int> CreateDatabaseUser(int[] databaseIds, string userName, string userPassword);
    Task<int> CreateDatabaseUser(int[] databaseIds, string userName);
    Task DeleteDatabaseUser(int databaseUserId);
    Task DeleteDatabaseUser(int databaseUserId, bool deleteDatabaseUser);
    Task<List<Domain.DatabaseUser>> GetDatabaseUsers();
    Task<Domain.DatabaseUser> GetDatabaseUser(int databaseUserId);
    Task UpdateDatabaseUser(
        int databaseUserId,
        int[] databaseIds,
        string databaseUserName,
        string databaseUserPassword,
        bool updateDatabase
    );
    Task UpdateDatabaseUser(
        int databaseUserId,
        int[] databaseIds,
        string databaseUserName,
        bool updateDatabase
    );
    Task UpdateDatabaseUser(
        int databaseUserId,
        int[] databaseIds,
        string databaseUserName,
        string databaseUserPassword
    );
    Task UpdateDatabaseUser(int databaseUserId, int[] databaseIds, string databaseUserName);
}
