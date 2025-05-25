using DbLocator.Domain;

namespace DbLocator
{
    public partial class Locator
    {
        /// <summary>
        /// Adds a new database user.
        /// </summary>
        /// <param name="databaseIds">
        /// The IDs of the databases to which the user will be assigned.
        /// </param>
        /// <param name="userName">
        /// The name of the user.
        /// </param>
        /// <param name="userPassword">
        /// The password for the user.
        /// </param>
        /// <param name="affectDatabase">
        /// A flag indicating whether to perform DDL operations on the database server.
        /// </param>
        /// <returns>
        /// The ID of the newly added database user.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when any of the specified database IDs are not found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when a user with the specified name already exists.
        /// </exception>
        public async Task<int> AddDatabaseUser(
            int[] databaseIds,
            string userName,
            string userPassword,
            bool affectDatabase
        )
        {
            return await _databaseUsers.AddDatabaseUser(
                databaseIds,
                userName,
                userPassword,
                affectDatabase
            );
        }

        /// <summary>
        /// Adds a new database user with the specified database ID, user name, and the option to create the user.
        /// The user password is not specified.
        /// </summary>
        /// <param name="databaseIds">
        /// The IDs of the databases to which the user belongs.
        /// </param>
        /// <param name="userName">
        /// The user name for the new database user.
        /// </param>
        /// <param name="affectDatabase">
        /// A flag indicating whether to perform DDL operations on the database server.
        /// </param>
        /// <returns>
        /// The ID of the newly created database user.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when any of the specified databases are not found.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the user name is invalid.
        /// </exception>
        public async Task<int> AddDatabaseUser(
            int[] databaseIds,
            string userName,
            bool affectDatabase
        )
        {
            return await _databaseUsers.AddDatabaseUser(databaseIds, userName, affectDatabase);
        }

        /// <summary>
        /// Adds a new database user with the specified database ID, user name, and user password.
        /// The option to create the user is not specified.
        /// </summary>
        /// <param name="databaseIds">
        /// The IDs of the databases to which the user belongs.
        /// </param>
        /// <param name="userName">
        /// The user name for the new database user.
        /// </param>
        /// <param name="userPassword">
        /// The password for the new database user.
        /// </param>
        /// <returns>
        /// The ID of the newly created database user.
        /// </returns>
        public async Task<int> AddDatabaseUser(
            int[] databaseIds,
            string userName,
            string userPassword
        )
        {
            return await _databaseUsers.AddDatabaseUser(databaseIds, userName, userPassword);
        }

        /// <summary>
        /// Adds a new database user with the specified database ID and user name.
        /// The user password and the option to create the user are not specified.
        /// </summary>
        /// <param name="databaseIds">
        /// The ID of the database to which the user belongs.
        /// </param>
        /// <param name="userName">
        /// The user name for the new database user.
        /// </param>
        /// <returns>
        /// The ID of the newly created database user.
        /// </returns>
        public async Task<int> AddDatabaseUser(int[] databaseIds, string userName)
        {
            return await _databaseUsers.AddDatabaseUser(databaseIds, userName);
        }

        /// <summary>
        /// Retrieves a list of all database users.
        /// </summary>
        /// <returns>
        /// A list of <see cref="DatabaseUser"/> objects representing all database users in the system.
        /// </returns>
        public async Task<List<DatabaseUser>> GetDatabaseUsers()
        {
            return await _databaseUsers.GetDatabaseUsers();
        }

        /// <summary>
        /// Retrieves a single database user by their ID.
        /// </summary>
        /// <param name="databaseUserId">
        /// The ID of the database user to retrieve.
        /// </param>
        /// <returns>
        /// A <see cref="DatabaseUser"/> object representing the database user with the specified ID.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when no database user is found with the given ID.
        /// </exception>
        public async Task<DatabaseUser> GetDatabaseUser(int databaseUserId)
        {
            return await _databaseUsers.GetDatabaseUser(databaseUserId);
        }

        /// <summary>
        /// Updates the details of an existing database user, including the user name, user password, and the option to update the database.
        /// </summary>
        /// <param name="databaseUserId">
        /// The ID of the database user to be updated.
        /// </param>
        /// <param name="databaseIds">
        /// The IDs of the databases to which the user belongs.
        /// </param>
        /// <param name="userName">
        /// The new user name for the database user.
        /// </param>
        /// <param name="userPassword">
        /// The new password for the database user.
        /// </param>
        /// <param name="affectDatabase">
        /// A flag indicating whether to perform DDL operations on the database server.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database user or any of the databases are not found.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the new user name or password is invalid.
        /// </exception>
        public async Task UpdateDatabaseUser(
            int databaseUserId,
            int[] databaseIds,
            string userName,
            string userPassword,
            bool affectDatabase
        )
        {
            await _databaseUsers.UpdateDatabaseUser(
                databaseUserId,
                databaseIds,
                userName,
                userPassword,
                affectDatabase
            );
        }

        /// <summary>
        /// Updates the name of an existing database user with the specified ID.
        /// The user password and the option to update the database are not specified.
        /// </summary>
        /// <param name="databaseUserId">
        /// The ID of the database user to be updated.
        /// </param>
        /// <param name="databaseIds">
        /// The IDs of the databases to which the user belongs.
        /// </param>
        /// <param name="userName">
        /// The new user name for the database user.
        /// </param>
        /// <param name="affectDatabase">
        /// A flag indicating whether to perform DDL operations on the database server.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database user or any of the databases are not found.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the new user name is invalid.
        /// </exception>
        public async Task UpdateDatabaseUser(
            int databaseUserId,
            int[] databaseIds,
            string userName,
            bool affectDatabase
        )
        {
            await _databaseUsers.UpdateDatabaseUser(
                databaseUserId,
                databaseIds,
                userName,
                affectDatabase
            );
        }

        /// <summary>
        /// Updates the user name and password of an existing database user with the specified ID.
        /// The option to update the database is not specified.
        /// </summary>
        /// <param name="databaseUserId">
        /// The ID of the database user to be updated.
        /// </param>
        /// <param name="databaseIds">
        /// The IDs of the databases to which the user belongs.
        /// </param>
        /// <param name="userName">
        /// The new user name for the database user.
        /// </param>
        /// <param name="userPassword">
        /// The new password for the database user.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public async Task UpdateDatabaseUser(
            int databaseUserId,
            int[] databaseIds,
            string userName,
            string userPassword
        )
        {
            await _databaseUsers.UpdateDatabaseUser(
                databaseUserId,
                databaseIds,
                userName,
                userPassword
            );
        }

        /// <summary>
        /// Updates the name of an existing database user with the specified ID.
        /// </summary>
        /// <param name="databaseUserId">
        /// The ID of the database user to be updated.
        /// </param>
        /// <param name="databaseIds">
        /// The IDs of the databases to which the user belongs.
        /// </param>
        /// <param name="userName">
        /// The new user name for the database user.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public async Task UpdateDatabaseUser(int databaseUserId, int[] databaseIds, string userName)
        {
            await _databaseUsers.UpdateDatabaseUser(databaseUserId, databaseIds, userName);
        }

        /// <summary>
        /// Deletes a database user with the specified ID.
        /// The option to also delete the associated database user is provided.
        /// </summary>
        /// <param name="databaseUserId">
        /// The ID of the database user to be deleted.
        /// </param>
        /// <param name="deleteDatabaseUser">
        /// A flag indicating whether the associated database user should also be deleted in the database.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database user is not found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to delete a user that is in use.
        /// </exception>
        public async Task DeleteDatabaseUser(int databaseUserId, bool deleteDatabaseUser)
        {
            await _databaseUsers.DeleteDatabaseUser(databaseUserId, deleteDatabaseUser);
        }

        /// <summary>
        /// Deletes a database user with the specified ID.
        /// </summary>
        /// <param name="databaseUserId">
        /// The ID of the database user to be deleted.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database user is not found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to delete a user that is in use.
        /// </exception>
        public async Task DeleteDatabaseUser(int databaseUserId)
        {
            await _databaseUsers.DeleteDatabaseUser(databaseUserId);
        }
    }
}
