#nullable enable

using DbLocator.Domain;
using FluentValidation;
using Microsoft.Data.SqlClient;

namespace DbLocator;

/// <summary>
/// This partial class contains all database user-related operations for the DbLocator system.
/// It provides comprehensive methods for managing database users, including:
/// - Creation of new database users with various configurations
/// - Retrieval of user information
/// - Updates to user settings and permissions
/// - Deletion of users
///
/// The database user management system supports:
/// - Multi-database user assignments
/// - Secure password management
/// - Database-level DDL operations
/// - User access control and permissions
/// </summary>
public partial class Locator
{
    /// <summary>
    /// Creates a new database user with the specified configuration.
    /// This method establishes a new database user that can access multiple databases
    /// with the provided credentials. The user can be created both in the DbLocator system
    /// and on the actual database servers.
    /// </summary>
    /// <param name="databaseIds">
    /// The unique identifiers of the databases to which the user will be assigned.
    /// These IDs must correspond to existing databases in the system. The user will
    /// be granted appropriate permissions on each specified database.
    /// </param>
    /// <param name="userName">
    /// The name of the user to be created. This should be a unique identifier that
    /// follows the database system's naming conventions and security requirements.
    /// </param>
    /// <param name="userPassword">
    /// The password for the new user. This should meet the database system's password
    /// complexity requirements and security policies. If null, the password must be
    /// set later using the UpdateDatabaseUser method.
    /// </param>
    /// <param name="affectDatabase">
    /// A flag indicating whether to perform DDL operations on the database server.
    /// When true, the user will be created both in the DbLocator system and on the
    /// actual database servers. When false, the user will only be registered in the
    /// DbLocator system. Defaults to true.
    /// </param>
    /// <returns>
    /// The unique identifier of the newly created database user. This ID can be used
    /// to reference the user in future operations and is used internally by the system.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when any of the specified database IDs are not found.
    /// This indicates that one or more databases do not exist in the system.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a user with the specified name already exists
    /// in any of the target databases. User names must be unique within each database.</exception>
    /// <exception cref="ValidationException">Thrown when the user name or password violates validation rules.
    /// This includes checks for proper formatting, length, and complexity requirements.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when DDL operations fail.
    /// This includes permission issues, connection problems, or database-specific errors.</exception>
    public async Task<int> CreateDatabaseUser(
        int[] databaseIds,
        string userName,
        string userPassword,
        bool affectDatabase
    )
    {
        return await _databaseUserService.CreateDatabaseUser(
            databaseIds,
            userName,
            userPassword,
            affectDatabase
        );
    }

    /// <summary>
    /// Retrieves a list of all database users in the system.
    /// This method returns comprehensive information about all database users, including their
    /// configuration, permissions, and associated metadata. The list can be used for administrative
    /// purposes or to audit the system's user configuration.
    /// </summary>
    /// <returns>
    /// A list of <see cref="DatabaseUser"/> objects, each containing detailed information about a user,
    /// including their name, assigned databases, and configuration settings.
    /// </returns>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or retrieving the user list.</exception>
    public async Task<List<DatabaseUser>> GetDatabaseUsers()
    {
        return await _databaseUserService.GetDatabaseUsers();
    }

    /// <summary>
    /// Retrieves a single database user by their unique identifier.
    /// This method returns detailed information about a specific database user, including their
    /// configuration, permissions, and associated metadata.
    /// </summary>
    /// <param name="databaseUserId">
    /// The unique identifier of the database user to retrieve. This ID must correspond to an
    /// existing database user in the system.
    /// </param>
    /// <returns>
    /// A <see cref="DatabaseUser"/> object containing detailed information about the user,
    /// including their name, assigned databases, and configuration settings.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when no database user is found with the given ID.
    /// This indicates that the user does not exist in the system.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or retrieving the user information.</exception>
    public async Task<DatabaseUser> GetDatabaseUser(int databaseUserId)
    {
        return await _databaseUserService.GetDatabaseUser(databaseUserId);
    }

    /// <summary>
    /// Updates an existing database user with the specified properties.
    /// This method allows changing any combination of the user's name, password, and database assignments.
    /// The operation can optionally update the user on the actual database servers.
    /// </summary>
    /// <param name="databaseUserId">
    /// The unique identifier of the database user to be updated. This ID must correspond to an
    /// existing database user in the system.
    /// </param>
    /// <param name="databaseUserName">
    /// The new name for the database user. This should be a unique identifier that
    /// follows the database system's naming conventions and security requirements.
    /// </param>
    /// <param name="databaseUserPassword">
    /// The new password for the database user. This should meet the database system's password
    /// complexity requirements and security policies.
    /// </param>
    /// <param name="databaseIds">
    /// The unique identifiers of the databases to which the user will be assigned.
    /// These IDs must correspond to existing databases in the system. The user's permissions
    /// will be updated on each specified database.
    /// </param>
    /// <param name="affectDatabase">
    /// A flag indicating whether to perform DDL operations on the database server.
    /// When true, the user will be updated both in the DbLocator system and on the
    /// actual database servers. When false, the user will only be updated in the
    /// DbLocator system.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the user
    /// has been successfully updated.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database user or any of the databases are not found.
    /// This indicates that the user or one or more databases do not exist in the system.</exception>
    /// <exception cref="ValidationException">Thrown when the new user name or password violates validation rules.
    /// This includes checks for proper formatting, length, and complexity requirements.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when DDL operations fail.
    /// This includes permission issues, connection problems, or database-specific errors.</exception>
    public async Task UpdateDatabaseUser(
        int databaseUserId,
        string? databaseUserName,
        string? databaseUserPassword,
        int[]? databaseIds,
        bool? affectDatabase
    )
    {
        await _databaseUserService.UpdateDatabaseUser(
            databaseUserId,
            databaseUserName,
            databaseUserPassword,
            databaseIds,
            affectDatabase
        );
    }

    /// <summary>
    /// Deletes a database user with the option to remove them from the database servers.
    /// This method permanently removes a database user from the system. The operation can
    /// optionally remove the user from the actual database servers as well.
    /// </summary>
    /// <param name="databaseUserId">
    /// The unique identifier of the database user to be deleted. This ID must correspond to an
    /// existing database user in the system.
    /// </param>
    /// <param name="deleteDatabaseUser">
    /// A flag indicating whether to perform DDL operations on the database server.
    /// When true, the user will be removed both from the DbLocator system and from the
    /// actual database servers. When false, the user will only be removed from the
    /// DbLocator system.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the user
    /// has been successfully deleted.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when no database user is found with the given ID.
    /// This indicates that the user does not exist in the system.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when DDL operations fail.
    /// This includes permission issues, connection problems, or database-specific errors.</exception>
    public async Task DeleteDatabaseUser(int databaseUserId, bool deleteDatabaseUser)
    {
        await _databaseUserService.DeleteDatabaseUser(databaseUserId, deleteDatabaseUser);
    }
}
