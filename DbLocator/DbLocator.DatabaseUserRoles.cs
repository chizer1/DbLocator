using DbLocator.Domain;
using FluentValidation;
using Microsoft.Data.SqlClient;

namespace DbLocator;

/// <summary>
/// This partial class contains all database user role-related operations for the DbLocator system.
/// It provides comprehensive methods for managing database user roles, including:
/// - Creation of new role assignments
/// - Removal of role assignments
/// - Role validation and enforcement
/// - Role-based access control
/// </summary>
public partial class Locator
{
    /// <summary>
    /// Creates a new role assignment for a database user with optional user update.
    /// This method establishes a new role assignment in the system and can optionally
    /// update the user's permissions on the actual database servers. The operation
    /// includes validation to ensure the role assignment is valid and not redundant.
    /// </summary>
    /// <param name="databaseUserId">
    /// The unique identifier of the database user to which the role will be assigned.
    /// This ID must correspond to an existing database user in the system.
    /// </param>
    /// <param name="userRole">
    /// The <see cref="DatabaseRole"/> to be assigned to the database user. This role
    /// must be a valid SQL Server database role and will be validated before assignment.
    /// </param>
    /// <param name="updateUser">
    /// A flag indicating whether to update the user's permissions on the database servers.
    /// When true, the role will be assigned both in the DbLocator system and on the
    /// actual database servers. When false, the role will only be registered in the
    /// DbLocator system.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the role
    /// has been successfully assigned.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database user is not found.
    /// This indicates that the user does not exist in the system.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the user already has the specified role.
    /// This prevents duplicate role assignments and ensures role uniqueness.</exception>
    /// <exception cref="ValidationException">Thrown when the role assignment violates validation rules.
    /// This includes checks for role compatibility, user permissions, and role dependencies.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when
    /// the role assignment operation fails. This includes permission issues, connection problems,
    /// or database-specific errors.</exception>
    public async Task CreateDatabaseUserRole(
        int databaseUserId,
        DatabaseRole userRole,
        bool updateUser
    )
    {
        await _databaseUserRoleService.CreateDatabaseUserRole(databaseUserId, userRole, updateUser);
    }

    /// <summary>
    /// Creates a new role assignment for a database user without updating the user.
    /// This method establishes a new role assignment in the system without modifying
    /// the user's permissions on the actual database servers. The operation includes
    /// validation to ensure the role assignment is valid and not redundant.
    /// </summary>
    /// <param name="databaseUserId">
    /// The unique identifier of the database user to which the role will be assigned.
    /// This ID must correspond to an existing database user in the system.
    /// </param>
    /// <param name="userRole">
    /// The <see cref="DatabaseRole"/> to be assigned to the database user. This role
    /// must be a valid SQL Server database role and will be validated before assignment.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the role
    /// has been successfully assigned.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database user is not found.
    /// This indicates that the user does not exist in the system.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the user already has the specified role.
    /// This prevents duplicate role assignments and ensures role uniqueness.</exception>
    /// <exception cref="ValidationException">Thrown when the role assignment violates validation rules.
    /// This includes checks for role compatibility, user permissions, and role dependencies.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when
    /// the role assignment operation fails. This includes permission issues, connection problems,
    /// or database-specific errors.</exception>
    public async Task CreateDatabaseUserRole(int databaseUserId, DatabaseRole userRole)
    {
        await _databaseUserRoleService.CreateDatabaseUserRole(databaseUserId, userRole, false);
    }

    /// <summary>
    /// Deletes a role assignment from a database user with optional database cleanup.
    /// This method removes a role assignment from the system and can optionally remove
    /// the role from the actual database servers. The operation includes validation to
    /// ensure the role removal is safe and does not violate system requirements.
    /// </summary>
    /// <param name="databaseUserId">
    /// The unique identifier of the database user from which the role will be removed.
    /// This ID must correspond to an existing database user in the system.
    /// </param>
    /// <param name="userRole">
    /// The <see cref="DatabaseRole"/> to be removed from the database user. This role
    /// must be currently assigned to the user and will be validated before removal.
    /// </param>
    /// <param name="affectDatabase">
    /// A flag indicating whether to remove the role from the database servers.
    /// When true, the role will be removed both from the DbLocator system and from the
    /// actual database servers. When false, the role will only be removed from the
    /// DbLocator system.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the role
    /// has been successfully removed.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database user is not found
    /// or when the user does not have the specified role. This indicates that either the user
    /// does not exist in the system or the role is not assigned to the user.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to delete a required role.
    /// This prevents removal of roles that are essential for system operation or user functionality.</exception>
    /// <exception cref="ValidationException">Thrown when the role removal violates validation rules.
    /// This includes checks for role dependencies, system requirements, and user permissions.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when
    /// the role removal operation fails. This includes permission issues, connection problems,
    /// or database-specific errors.</exception>
    public async Task DeleteDatabaseUserRole(
        int databaseUserId,
        DatabaseRole userRole,
        bool affectDatabase
    )
    {
        await _databaseUserRoleService.DeleteDatabaseUserRole(
            databaseUserId,
            userRole,
            affectDatabase
        );
    }

    /// <summary>
    /// Deletes a role assignment from a database user without database cleanup.
    /// This method removes a role assignment from the system without modifying the
    /// user's permissions on the actual database servers. The operation includes
    /// validation to ensure the role removal is safe and does not violate system requirements.
    /// </summary>
    /// <param name="databaseUserId">
    /// The unique identifier of the database user from which the role will be removed.
    /// This ID must correspond to an existing database user in the system.
    /// </param>
    /// <param name="userRole">
    /// The <see cref="DatabaseRole"/> to be removed from the database user. This role
    /// must be currently assigned to the user and will be validated before removal.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the role
    /// has been successfully removed.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when the specified database user is not found
    /// or when the user does not have the specified role. This indicates that either the user
    /// does not exist in the system or the role is not assigned to the user.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to delete a required role.
    /// This prevents removal of roles that are essential for system operation or user functionality.</exception>
    /// <exception cref="ValidationException">Thrown when the role removal violates validation rules.
    /// This includes checks for role dependencies, system requirements, and user permissions.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or when
    /// the role removal operation fails. This includes permission issues, connection problems,
    /// or database-specific errors.</exception>
    public async Task DeleteDatabaseUserRole(int databaseUserId, DatabaseRole userRole)
    {
        await _databaseUserRoleService.DeleteDatabaseUserRole(databaseUserId, userRole);
    }
}
