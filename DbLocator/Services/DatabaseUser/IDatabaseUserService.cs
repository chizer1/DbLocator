namespace DbLocator.Services.DatabaseUser;

internal interface IDatabaseUserService
{
    /// <param name="databaseIds">The IDs of the databases to which the user will be assigned.</param>
    /// <param name="userName">The name of the user.</param>
    /// <param name="userPassword">The password for the user.</param>
    /// <param name="affectDatabase">
    /// A flag indicating whether to perform DDL operations on the database server. If not provided, defaults to true.
    /// </param>
    Task<int> AddDatabaseUser(
        int[] databaseIds,
        string userName,
        string userPassword,
        bool affectDatabase = true
    );

    /// <param name="databaseIds">The IDs of the databases to which the user will be assigned.</param>
    /// <param name="userName">The name of the user.</param>
    /// <param name="affectDatabase">
    /// A flag indicating whether to perform DDL operations on the database server. If not provided, defaults to true.
    /// </param>
    Task<int> AddDatabaseUser(int[] databaseIds, string userName, bool affectDatabase = true);
    Task<int> AddDatabaseUser(int[] databaseIds, string userName, string userPassword);
    Task<int> AddDatabaseUser(int[] databaseIds, string userName);
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
    Task UpdateDatabaseUser(
        int databaseUserId,
        string databaseUserName,
        string databaseUserPassword
    );
    Task UpdateDatabaseUser(int databaseUserId, string databaseUserName);
    Task UpdateDatabaseUser(int databaseUserId, int[] databaseIds);
}
