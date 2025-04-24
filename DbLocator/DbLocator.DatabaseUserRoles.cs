using DbLocator.Domain;

namespace DbLocator
{
    public partial class Locator
    {
        /// <summary>
        /// Adds a new role to a database user. This method also updates the user if specified.
        /// </summary>
        /// <param name="databaseUserId">
        /// The ID of the database user to which the role will be assigned.
        /// </param>
        /// <param name="userRole">
        /// The <see cref="DatabaseRole"/> to be assigned to the database user.
        /// </param>
        /// <param name="updateUser">
        /// A flag indicating whether to update the user after the role is added.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public async Task AddDatabaseUserRole(
            int databaseUserId,
            DatabaseRole userRole,
            bool updateUser
        )
        {
            await _databaseUserRoles.AddDatabaseUserRole(databaseUserId, userRole, updateUser);
        }

        /// <summary>
        /// Adds a new role to a database user. This method does not update the user.
        /// </summary>
        /// <param name="databaseUserId">
        /// The ID of the database user to which the role will be assigned.
        /// </param>
        /// <param name="userRole">
        /// The <see cref="DatabaseRole"/> to be assigned to the database user.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public async Task AddDatabaseUserRole(int databaseUserId, DatabaseRole userRole)
        {
            await _databaseUserRoles.AddDatabaseUserRole(databaseUserId, userRole);
        }

        /// <summary>
        /// Gets all database user roles.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public async Task<List<DatabaseUserRole>> GetDatabaseUserRoles()
        {
            return await _databaseUserRoles.GetDatabaseUserRoles();
        }

        /// <summary>
        /// Deletes a role from a database user. This method also removes the role from the database if specified.
        /// </summary>
        /// <param name="databaseUserId">
        /// The ID of the database user from which the role will be removed.
        /// </param>
        /// <param name="userRole">
        /// The <see cref="DatabaseRole"/> to be deleted from the database user.
        /// </param>
        /// <param name="deleteDatabaseUserRole">
        /// A flag indicating whether to delete the role from the database.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public async Task DeleteDatabaseUserRole(
            int databaseUserId,
            DatabaseRole userRole,
            bool deleteDatabaseUserRole
        )
        {
            await _databaseUserRoles.DeleteDatabaseUserRole(
                databaseUserId,
                userRole,
                deleteDatabaseUserRole
            );
        }

        /// <summary>
        /// Deletes a role from a database user. This method does not delete the role from the database.
        /// </summary>
        /// <param name="databaseUserId">
        /// The ID of the database user from which the role will be removed.
        /// </param>
        /// <param name="userRole">
        /// The <see cref="DatabaseRole"/> to be deleted from the database user.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public async Task DeleteDatabaseUserRole(int databaseUserId, DatabaseRole userRole)
        {
            await _databaseUserRoles.DeleteDatabaseUserRole(databaseUserId, userRole);
        }
    }
}
