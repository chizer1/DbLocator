using DbLocator.Domain;
using FluentValidation;
using Microsoft.Data.SqlClient;

namespace DbLocator;

/// <summary>
/// This partial class contains all tenant-related operations for the DbLocator system.
/// It provides comprehensive methods for managing tenants, including:
/// - Creation of new tenants with various configurations
/// - Retrieval of tenant information
/// - Updates to tenant settings and properties
/// - Deletion of tenants
///
/// The tenant management system supports:
/// - Multi-tenant database environments
/// - Different tenant statuses (active, inactive, etc.)
/// - Unique tenant codes for identification
/// - Tenant-specific database access control
/// </summary>
public partial class Locator
{
    /// <summary>
    /// Creates a new tenant with the specified name, code, and status.
    /// This method establishes a new tenant in the system with the provided configuration.
    /// The tenant will be able to access databases based on their assigned permissions.
    /// </summary>
    /// <param name="tenantName">
    /// The name of the tenant to be created. This should be a descriptive name that identifies the tenant.
    /// The name should be unique and meaningful for administrative purposes.
    /// </param>
    /// <param name="tenantCode">
    /// The unique code assigned to the tenant. This code is used as a short identifier for the tenant
    /// and must be unique across all tenants in the system. It is typically used in connection strings
    /// and configuration settings.
    /// </param>
    /// <param name="tenantStatus">
    /// The initial status of the tenant (e.g., Active, Inactive). This status determines whether the tenant
    /// can access the system and its databases. The status can be updated later using the UpdateTenant method.
    /// </param>
    /// <returns>
    /// The unique identifier of the newly created tenant. This ID can be used to reference the tenant
    /// in future operations and is used internally by the system.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the tenant name or code is null, empty, or invalid.
    /// The name and code must follow the system's validation rules.</exception>
    /// <exception cref="ValidationException">Thrown when the tenant name or code violates business validation rules.
    /// This includes checks for proper formatting and content restrictions.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a tenant with the same code already exists.
    /// Tenant codes must be unique across the system.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or creating the tenant record.</exception>
    public async Task<int> CreateTenant(string tenantName, string tenantCode, Status tenantStatus)
    {
        return await _tenantService.CreateTenant(tenantName, tenantCode, tenantStatus);
    }

    /// <summary>
    /// Creates a new tenant with the specified name and status.
    /// This is a convenience method that creates a tenant with a system-generated code.
    /// The tenant will be able to access databases based on their assigned permissions.
    /// </summary>
    /// <param name="tenantName">
    /// The name of the tenant to be created. This should be a descriptive name that identifies the tenant.
    /// The name should be unique and meaningful for administrative purposes.
    /// </param>
    /// <param name="tenantStatus">
    /// The initial status of the tenant (e.g., Active, Inactive). This status determines whether the tenant
    /// can access the system and its databases. The status can be updated later using the UpdateTenant method.
    /// </param>
    /// <returns>
    /// The unique identifier of the newly created tenant. This ID can be used to reference the tenant
    /// in future operations and is used internally by the system.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the tenant name is null, empty, or invalid.
    /// The name must follow the system's validation rules.</exception>
    /// <exception cref="ValidationException">Thrown when the tenant name violates business validation rules.
    /// This includes checks for proper formatting and content restrictions.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or creating the tenant record.</exception>
    public async Task<int> CreateTenant(string tenantName, Status tenantStatus)
    {
        return await _tenantService.CreateTenant(tenantName, tenantStatus);
    }

    /// <summary>
    /// Creates a new tenant with only the specified name.
    /// This is the simplest method to create a tenant, using default values for code and status.
    /// The tenant will be created with an Active status and a system-generated code.
    /// </summary>
    /// <param name="tenantName">
    /// The name of the tenant to be created. This should be a descriptive name that identifies the tenant.
    /// The name should be unique and meaningful for administrative purposes.
    /// </param>
    /// <returns>
    /// The unique identifier of the newly created tenant. This ID can be used to reference the tenant
    /// in future operations and is used internally by the system.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when the tenant name is null, empty, or invalid.
    /// The name must follow the system's validation rules.</exception>
    /// <exception cref="ValidationException">Thrown when the tenant name violates business validation rules.
    /// This includes checks for proper formatting and content restrictions.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or creating the tenant record.</exception>
    public async Task<int> CreateTenant(string tenantName)
    {
        return await _tenantService.CreateTenant(tenantName);
    }

    /// <summary>
    /// Retrieves a list of all tenants in the system.
    /// This method returns comprehensive information about all tenants, including their configuration,
    /// status, and associated metadata. The list can be used for administrative purposes or to audit
    /// the system's tenant configuration.
    /// </summary>
    /// <returns>
    /// A list of <see cref="Tenant"/> objects, each containing detailed information about a tenant,
    /// including their name, code, status, and configuration settings.
    /// </returns>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or retrieving the tenant list.</exception>
    public async Task<List<Tenant>> GetTenants()
    {
        return await _tenantService.GetTenants();
    }

    /// <summary>
    /// Retrieves a single tenant by their unique identifier.
    /// This method returns detailed information about a specific tenant, including their configuration,
    /// status, and associated metadata.
    /// </summary>
    /// <param name="tenantId">
    /// The unique identifier of the tenant to retrieve. This ID must correspond to an existing tenant in the system.
    /// </param>
    /// <returns>
    /// A <see cref="Tenant"/> object containing detailed information about the tenant,
    /// including their name, code, status, and configuration settings.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when no tenant is found with the given ID.
    /// This indicates that the tenant does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the tenant ID is invalid.
    /// The ID must be a positive integer.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or retrieving the tenant information.</exception>
    public async Task<Tenant> GetTenant(int tenantId)
    {
        return await _tenantService.GetTenant(tenantId);
    }

    /// <summary>
    /// Retrieves a single tenant by their unique code.
    /// This method returns detailed information about a specific tenant, including their configuration,
    /// status, and associated metadata. The code is typically a more user-friendly identifier than the tenant ID.
    /// </summary>
    /// <param name="tenantCode">
    /// The unique code of the tenant to retrieve. This code must correspond to an existing tenant in the system.
    /// The code is case-sensitive and must match exactly.
    /// </param>
    /// <returns>
    /// A <see cref="Tenant"/> object containing detailed information about the tenant,
    /// including their name, code, status, and configuration settings.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when no tenant is found with the given code.
    /// This indicates that the tenant does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the tenant code is null, empty, or invalid.
    /// The code must follow the system's validation rules.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or retrieving the tenant information.</exception>
    public async Task<Tenant> GetTenant(string tenantCode)
    {
        return await _tenantService.GetTenant(tenantCode);
    }

    /// <summary>
    /// Updates the name of an existing tenant.
    /// This method allows changing the display name of a tenant while preserving their other settings.
    /// The operation is performed asynchronously and updates only the tenant's name.
    /// </summary>
    /// <param name="tenantId">
    /// The unique identifier of the tenant to be updated. This ID must correspond to an existing tenant in the system.
    /// </param>
    /// <param name="tenantName">
    /// The new name for the tenant. This should be a descriptive name that identifies the tenant.
    /// The name should be unique and meaningful for administrative purposes.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the tenant has been successfully updated.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when no tenant is found with the given ID.
    /// This indicates that the tenant does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the tenant name is null, empty, or invalid.
    /// The name must follow the system's validation rules.</exception>
    /// <exception cref="ValidationException">Thrown when the tenant name violates business validation rules.
    /// This includes checks for proper formatting and content restrictions.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or updating the tenant.</exception>
    public async Task UpdateTenant(int tenantId, string tenantName)
    {
        await _tenantService.UpdateTenant(tenantId, tenantName);
    }

    /// <summary>
    /// Updates the status of an existing tenant.
    /// This method allows changing the operational status of a tenant (e.g., Active, Inactive).
    /// The operation is performed asynchronously and updates only the tenant's status.
    /// </summary>
    /// <param name="tenantId">
    /// The unique identifier of the tenant to be updated. This ID must correspond to an existing tenant in the system.
    /// </param>
    /// <param name="tenantStatus">
    /// The new status for the tenant (e.g., Active, Inactive). This status determines whether the tenant
    /// can access the system and its databases.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the tenant has been successfully updated.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when no tenant is found with the given ID.
    /// This indicates that the tenant does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the tenant ID is invalid.
    /// The ID must be a positive integer.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or updating the tenant.</exception>
    public async Task UpdateTenant(int tenantId, Status tenantStatus)
    {
        await _tenantService.UpdateTenant(tenantId, tenantStatus);
    }

    /// <summary>
    /// Updates the name and code of an existing tenant.
    /// This method allows changing both the display name and the unique code of a tenant.
    /// The operation is performed asynchronously and updates both the tenant's name and code.
    /// </summary>
    /// <param name="tenantId">
    /// The unique identifier of the tenant to be updated. This ID must correspond to an existing tenant in the system.
    /// </param>
    /// <param name="tenantName">
    /// The new name for the tenant. This should be a descriptive name that identifies the tenant.
    /// The name should be unique and meaningful for administrative purposes.
    /// </param>
    /// <param name="tenantCode">
    /// The new code for the tenant. This code is used as a short identifier for the tenant
    /// and must be unique across all tenants in the system. It is typically used in connection strings
    /// and configuration settings.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the tenant has been successfully updated.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when no tenant is found with the given ID.
    /// This indicates that the tenant does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the tenant name or code is null, empty, or invalid.
    /// Both the name and code must follow the system's validation rules.</exception>
    /// <exception cref="ValidationException">Thrown when the tenant name or code violates business validation rules.
    /// This includes checks for proper formatting and content restrictions.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a tenant with the same code already exists.
    /// Tenant codes must be unique across the system.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or updating the tenant.</exception>
    public async Task UpdateTenant(int tenantId, string tenantName, string tenantCode)
    {
        await _tenantService.UpdateTenant(tenantId, tenantName, tenantCode);
    }

    /// <summary>
    /// Updates all properties of an existing tenant.
    /// This method allows changing the name, code, and status of a tenant in a single operation.
    /// The operation is performed asynchronously and updates all specified tenant properties.
    /// </summary>
    /// <param name="tenantId">
    /// The unique identifier of the tenant to be updated. This ID must correspond to an existing tenant in the system.
    /// </param>
    /// <param name="tenantName">
    /// The new name for the tenant. This should be a descriptive name that identifies the tenant.
    /// The name should be unique and meaningful for administrative purposes.
    /// </param>
    /// <param name="tenantCode">
    /// The new code for the tenant. This code is used as a short identifier for the tenant
    /// and must be unique across all tenants in the system. It is typically used in connection strings
    /// and configuration settings.
    /// </param>
    /// <param name="tenantStatus">
    /// The new status for the tenant (e.g., Active, Inactive). This status determines whether the tenant
    /// can access the system and its databases.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the tenant has been successfully updated.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when no tenant is found with the given ID.
    /// This indicates that the tenant does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the tenant name or code is null, empty, or invalid.
    /// Both the name and code must follow the system's validation rules.</exception>
    /// <exception cref="ValidationException">Thrown when the tenant name or code violates business validation rules.
    /// This includes checks for proper formatting and content restrictions.</exception>
    /// <exception cref="InvalidOperationException">Thrown when a tenant with the same code already exists.
    /// Tenant codes must be unique across the system.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or updating the tenant.</exception>
    public async Task UpdateTenant(
        int tenantId,
        string tenantName,
        string tenantCode,
        Status tenantStatus
    )
    {
        await _tenantService.UpdateTenant(tenantId, tenantName, tenantCode, tenantStatus);
    }

    /// <summary>
    /// Deletes a tenant by their unique identifier.
    /// This method permanently removes a tenant from the system. The operation is irreversible
    /// and will remove all associated tenant data. Before deletion, ensure that the tenant
    /// has no active connections or dependencies.
    /// </summary>
    /// <param name="tenantId">
    /// The unique identifier of the tenant to be deleted. This ID must correspond to an existing tenant in the system.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task completes when the tenant has been successfully deleted.
    /// </returns>
    /// <exception cref="KeyNotFoundException">Thrown when no tenant is found with the given ID.
    /// This indicates that the tenant does not exist in the system.</exception>
    /// <exception cref="ArgumentException">Thrown when the tenant ID is invalid.
    /// The ID must be a positive integer.</exception>
    /// <exception cref="InvalidOperationException">Thrown when attempting to delete a tenant that has active connections
    /// or dependencies. These must be removed before the tenant can be deleted.</exception>
    /// <exception cref="SqlException">Thrown when there is an error connecting to the database or deleting the tenant.</exception>
    public async Task DeleteTenant(int tenantId)
    {
        await _tenantService.DeleteTenant(tenantId);
    }
}
