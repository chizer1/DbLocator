#nullable enable

using DbLocator.Domain;
using Microsoft.Data.SqlClient;

namespace DbLocator
{
    public partial class Locator
    {
        /// <summary>
        /// Retrieves a SQL connection based on the provided connection ID and optional roles.
        /// </summary>
        /// <param name="connectionId">
        /// The ID of the connection to retrieve.
        /// </param>
        /// <param name="roles">
        /// Optional array of database roles to filter the connection by.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation, with a <see cref="SqlConnection"/> result.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified connection is not found.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when invalid query parameters are provided.
        /// </exception>
        public async Task<SqlConnection> GetConnection(
            int connectionId,
            DatabaseRole[]? roles = null
        )
        {
            return await _connectionService.GetConnection(connectionId, roles);
        }

        /// <summary>
        /// Retrieves a SQL connection based on the provided tenant ID, database type ID, and optional roles.
        /// </summary>
        /// <param name="tenantId">
        /// The ID of the tenant requesting the connection.
        /// </param>
        /// <param name="databaseTypeId">
        /// The ID of the database type for the connection.
        /// </param>
        /// <param name="roles">
        /// Optional array of database roles to filter the connection by.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation, with a <see cref="SqlConnection"/> result.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified tenant or database type is not found.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when invalid query parameters are provided.
        /// </exception>
        public async Task<SqlConnection> GetConnection(
            int tenantId,
            int databaseTypeId,
            DatabaseRole[]? roles = null
        )
        {
            return await _connectionService.GetConnection(tenantId, databaseTypeId, roles);
        }

        /// <summary>
        /// Retrieves a SQL connection based on the provided tenant code, database type ID, and optional roles.
        /// </summary>
        /// <param name="tenantCode">
        /// The code of the tenant requesting the connection.
        /// </param>
        /// <param name="databaseTypeId">
        /// The ID of the database type for the connection.
        /// </param>
        /// <param name="roles">
        /// Optional array of database roles to filter the connection by.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation, with a <see cref="SqlConnection"/> result.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified tenant or database type is not found.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when invalid query parameters are provided.
        /// </exception>
        public async Task<SqlConnection> GetConnection(
            string tenantCode,
            int databaseTypeId,
            DatabaseRole[]? roles = null
        )
        {
            return await _connectionService.GetConnection(tenantCode, databaseTypeId, roles);
        }

        /// <summary>
        /// Retrieves a list of all connections.
        /// </summary>
        /// <returns>
        /// A task representing the asynchronous operation, with a list of <see cref="Connection"/> objects.
        /// </returns>
        public async Task<List<Connection>> GetConnections()
        {
            return await _connectionService.GetConnections();
        }

        /// <summary>
        /// Adds a new connection between the specified tenant and database.
        /// </summary>
        /// <param name="tenantId">
        /// The ID of the tenant for the connection.
        /// </param>
        /// <param name="databaseId">
        /// The ID of the database to connect to.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation, with the ID of the newly added connection.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified tenant or database is not found.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when attempting to add a duplicate connection.
        /// </exception>
        public async Task<int> AddConnection(int tenantId, int databaseId)
        {
            return await _connectionService.AddConnection(tenantId, databaseId);
        }

        /// <summary>
        /// Deletes a connection by its ID.
        /// </summary>
        /// <param name="connectionId">
        /// The ID of the connection to be deleted.
        /// </param>
        /// <returns>
        /// A task representing the asynchronous operation.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when the specified connection is not found.
        /// </exception>
        public async Task DeleteConnection(int connectionId)
        {
            await _connectionService.DeleteConnection(connectionId);
        }
    }
}
