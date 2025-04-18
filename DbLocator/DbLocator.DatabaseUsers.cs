using DbLocator.Domain;

namespace DbLocator
{
    public partial class Locator
    {
        /// <summary>
        /// Adds a new database user with the specified database ID, user name, user password, and the option to create the user.
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
        /// <param name="createUser">
        /// A flag indicating whether to create the user in the database.
        /// </param>
        /// <returns>
        /// The ID of the newly created database user.
        /// </returns>
        public async Task<int> AddDatabaseUser(
            List<int> databaseIds,
            string userName,
            string userPassword,
            bool createUser
        )
        {
            return await _databaseUsers.AddDatabaseUser(
                databaseIds,
                userName,
                userPassword,
                createUser
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
        /// <param name="createUser">
        /// A flag indicating whether to create the user in the database.
        /// </param>
        /// <returns>
        /// The ID of the newly created database user.
        /// </returns>
        public async Task<int> AddDatabaseUser(
            List<int> databaseIds,
            string userName,
            bool createUser
        )
        {
            return await _databaseUsers.AddDatabaseUser(databaseIds, userName, createUser);
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
            List<int> databaseIds,
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
        public async Task<int> AddDatabaseUser(List<int> databaseIds, string userName)
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
        /// Updates the details of an existing database user, including the user name, user password, and the option to update the database.
        /// </summary>
        /// <param name="databaseUserId">
        /// The ID of the database user to be updated.
        /// </param>
        /// <param name="databaseIds">
        /// The IDs of the databases to which the user belongs.
        /// </param>
        /// <param name="databaseUserName">
        /// The new user name for the database user.
        /// </param>
        /// <param name="databaseUserPassword">
        /// The new password for the database user.
        /// </param>
        /// <param name="updateDatabase">
        /// A flag indicating whether the associated database should also be updated.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public async Task UpdateDatabaseUser(
            int databaseUserId,
            List<int> databaseIds,
            string databaseUserName,
            string databaseUserPassword,
            bool updateDatabase
        )
        {
            await _databaseUsers.UpdateDatabaseUser(
                databaseUserId,
                databaseIds,
                databaseUserName,
                databaseUserPassword,
                updateDatabase
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
        /// <param name="databaseUserName">
        /// The new user name for the database user.
        /// </param>
        /// <param name="updateDatabase">
        /// A flag indicating whether the associated database should also be updated.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public async Task UpdateDatabaseUser(
            int databaseUserId,
            List<int> databaseIds,
            string databaseUserName,
            bool updateDatabase
        )
        {
            await _databaseUsers.UpdateDatabaseUser(
                databaseUserId,
                databaseIds,
                databaseUserName,
                updateDatabase
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
        /// <param name="databaseUserName">
        /// The new user name for the database user.
        /// </param>
        /// <param name="databaseUserPassword">
        /// The new password for the database user.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public async Task UpdateDatabaseUser(
            int databaseUserId,
            List<int> databaseIds,
            string databaseUserName,
            string databaseUserPassword
        )
        {
            await _databaseUsers.UpdateDatabaseUser(
                databaseUserId,
                databaseIds,
                databaseUserName,
                databaseUserPassword
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
        /// <param name="databaseUserName">
        /// The new user name for the database user.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public async Task UpdateDatabaseUser(
            int databaseUserId,
            List<int> databaseIds,
            string databaseUserName
        )
        {
            await _databaseUsers.UpdateDatabaseUser(databaseUserId, databaseIds, databaseUserName);
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
        public async Task DeleteDatabaseUser(int databaseUserId)
        {
            await _databaseUsers.DeleteDatabaseUser(databaseUserId);
        }
    }
}
