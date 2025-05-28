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
        /// A flag indicating whether to update the user.
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
            await _databaseUserRoleService.AddDatabaseUserRole(
                databaseUserId,
                userRole,
                updateUser
            );
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
            await _databaseUserRoleService.AddDatabaseUserRole(databaseUserId, userRole);
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
        /// A flag indicating whether to delete the database user role.
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
            await _databaseUserRoleService.DeleteDatabaseUserRole(
                databaseUserId,
                userRole,
                deleteDatabaseUserRole
            );
        }

        /// <summary>
        /// Deletes a role from a database user. This method does not remove the role from the database.
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
            await _databaseUserRoleService.DeleteDatabaseUserRole(databaseUserId, userRole);
        }
    }
}
