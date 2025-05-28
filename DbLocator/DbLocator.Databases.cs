using DbLocator.Domain;

namespace DbLocator
{
    public partial class Locator
    {
        /// <summary>
        /// Adds a new database.
        /// </summary>
        /// <param name="databaseName">
        /// The name of the database.
        /// </param>
        /// <param name="databaseServerId">
        /// The ID of the database server where the database is located.
        /// </param>
        /// <param name="databaseTypeId">
        /// The ID of the logical database type (e.g., Operational, Analytical).
        /// </param>
        /// <param name="databaseStatus">
        /// The status of the database (e.g., Active, Inactive).
        /// </param>
        /// <returns>
        /// The ID of the newly added database.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database server or database type is not found.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the database name is invalid or when attempting to create a database with invalid parameters.
        /// </exception>
        public async Task<int> AddDatabase(
            string databaseName,
            int databaseServerId,
            byte databaseTypeId,
            Status databaseStatus
        )
        {
            return await _databaseService.AddDatabase(
                databaseName,
                databaseServerId,
                databaseTypeId,
                databaseStatus
            );
        }

        /// <summary>
        /// Adds a new database with an option to create the database.
        /// </summary>
        /// <param name="databaseName">
        /// The name of the database.
        /// </param>
        /// <param name="databaseServerId">
        /// The ID of the database server where the database is located.
        /// </param>
        /// <param name="databaseTypeId">
        /// The ID of the database type.
        /// </param>
        /// <param name="databaseStatus">
        /// The status of the database.
        /// </param>
        /// <param name="createDatabase">
        /// A flag indicating whether to perform DDL operations on the database server.
        /// </param>
        /// <returns>
        /// The ID of the newly added database.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database server or database type is not found.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the database name is invalid or when attempting to create a database with invalid parameters.
        /// </exception>
        public async Task<int> AddDatabase(
            string databaseName,
            int databaseServerId,
            byte databaseTypeId,
            Status databaseStatus,
            bool createDatabase
        )
        {
            return await _databaseService.AddDatabase(
                databaseName,
                databaseServerId,
                databaseTypeId,
                databaseStatus,
                createDatabase
            );
        }

        /// <summary>
        /// Adds a new database with the default status of Active.
        /// </summary>
        /// <param name="databaseName">
        /// The name of the database.
        /// </param>
        /// <param name="databaseServerId">
        /// The ID of the database server where the database is located.
        /// </param>
        /// <param name="databaseTypeId">
        /// The ID of the database type.
        /// </param>
        /// <param name="createDatabase">
        /// A flag indicating whether to perform DDL operations on the database server.
        /// </param>
        /// <returns>
        /// The ID of the newly added database.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database server or database type is not found.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when the database name is invalid or when attempting to create a database with invalid parameters.
        /// </exception>
        public async Task<int> AddDatabase(
            string databaseName,
            int databaseServerId,
            byte databaseTypeId,
            bool createDatabase
        )
        {
            return await _databaseService.AddDatabase(
                databaseName,
                databaseServerId,
                databaseTypeId,
                createDatabase
            );
        }

        /// <summary>
        /// Adds a new database with specified trusted connection settings.
        /// </summary>
        /// <param name="databaseName">The name of the database to be added.</param>
        /// <param name="databaseServerId">The ID of the database server where the database will be created.</param>
        /// <param name="databaseTypeId">The ID of the database type.</param>
        /// <param name="createDatabase">A flag indicating whether to perform DDL operations on the database server.</param>
        /// <param name="useTrustedConnection">A flag indicating whether to use Windows authentication for the database connection.</param>
        /// <returns>The ID of the newly created database.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when the specified database server or type is not found.</exception>
        public async Task<int> AddDatabase(
            string databaseName,
            int databaseServerId,
            byte databaseTypeId,
            bool createDatabase,
            bool useTrustedConnection
        )
        {
            return await _databaseService.AddDatabase(
                databaseName,
                databaseServerId,
                databaseTypeId,
                createDatabase,
                useTrustedConnection
            );
        }

        /// <summary>
        /// Retrieves a database by its ID.
        /// </summary>
        /// <param name="databaseId">
        /// The ID of the database to be retrieved.
        /// </param>
        /// <returns>
        /// A <see cref="Database"/> object representing the database.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database is not found.
        /// </exception>
        public async Task<Database> GetDatabase(int databaseId)
        {
            return await _databaseService.GetDatabase(databaseId);
        }

        /// <summary>
        /// Retrieves a list of all available databases.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Database"/> representing the available databases.
        /// </returns>
        public async Task<List<Database>> GetDatabases()
        {
            return await _databaseService.GetDatabases();
        }

        /// <summary>
        /// Updates an existing database.
        /// </summary>
        /// <param name="databaseId">
        /// The ID of the database to be updated.
        /// </param>
        /// <param name="databaseName">
        /// The new name for the database.
        /// </param>
        /// <param name="databaseServerId">
        /// The ID of the database server where the database is located.
        /// </param>
        /// <param name="databaseTypeId">
        /// The ID of the database type.
        /// </param>
        /// <param name="databaseStatus">
        /// The status of the database.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database is not found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to update a database with invalid parameters.
        /// </exception>
        public async Task UpdateDatabase(
            int databaseId,
            string databaseName,
            int databaseServerId,
            byte databaseTypeId,
            Status databaseStatus
        )
        {
            await _databaseService.UpdateDatabase(
                databaseId,
                databaseName,
                databaseServerId,
                databaseTypeId,
                databaseStatus
            );
        }

        /// <summary>
        /// Updates the database server for an existing database.
        /// </summary>
        /// <param name="databaseId">
        /// The ID of the database to be updated.
        /// </param>
        /// <param name="databaseServerId">
        /// The ID of the new database server.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public async Task UpdateDatabase(int databaseId, int databaseServerId)
        {
            await _databaseService.UpdateDatabase(databaseId, databaseServerId);
        }

        /// <summary>
        /// Updates the database type for an existing database.
        /// </summary>
        /// <param name="databaseId">
        /// The ID of the database to be updated.
        /// </param>
        /// <param name="databaseTypeId">
        /// The ID of the new database type.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public async Task UpdateDatabase(int databaseId, byte databaseTypeId)
        {
            await _databaseService.UpdateDatabase(databaseId, databaseTypeId);
        }

        /// <summary>
        /// Updates the name for an existing database.
        /// </summary>
        /// <param name="databaseId">
        /// The ID of the database to be updated.
        /// </param>
        /// <param name="databaseName">
        /// The new name for the database.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database is not found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to update a database with invalid parameters.
        /// </exception>
        public async Task UpdateDatabase(int databaseId, string databaseName)
        {
            await _databaseService.UpdateDatabase(databaseId, databaseName);
        }

        /// <summary>
        /// Updates the status for an existing database.
        /// </summary>
        /// <param name="databaseId">
        /// The ID of the database to be updated.
        /// </param>
        /// <param name="databaseStatus">
        /// The new status for the database.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database is not found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to update a database with invalid parameters.
        /// </exception>
        public async Task UpdateDatabase(int databaseId, Status databaseStatus)
        {
            await _databaseService.UpdateDatabase(databaseId, databaseStatus);
        }

        /// <summary>
        /// Updates the trusted connection flag for an existing database.
        /// </summary>
        /// <param name="databaseId">
        /// The ID of the database to be updated.
        /// </param>
        /// <param name="useTrustedConnection">
        /// A flag indicating whether to use a trusted connection.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database is not found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to update a database with invalid parameters.
        /// </exception>
        public async Task UpdateDatabase(int databaseId, bool useTrustedConnection)
        {
            await _databaseService.UpdateDatabase(databaseId, useTrustedConnection);
        }

        /// <summary>
        /// Deletes a specified database.
        /// </summary>
        /// <param name="databaseId">
        /// The ID of the database to be deleted.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database is not found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to delete a database that is in use.
        /// </exception>
        public async Task DeleteDatabase(int databaseId)
        {
            await _databaseService.DeleteDatabase(databaseId);
        }

        /// <summary>
        /// Deletes a specified database with an option to delete it completely.
        /// </summary>
        /// <param name="databaseId">
        /// The ID of the database to be deleted.
        /// </param>
        /// <param name="deleteDatabase">
        /// A flag indicating whether to delete the database completely.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified database is not found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to delete a database that is in use.
        /// </exception>
        public async Task DeleteDatabase(int databaseId, bool deleteDatabase)
        {
            await _databaseService.DeleteDatabase(databaseId, deleteDatabase);
        }
    }
}
