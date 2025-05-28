using DbLocator.Domain;

namespace DbLocator
{
    public partial class Locator
    {
        /// <summary>
        /// Adds a new tenant with the specified name, code, and status.
        /// </summary>
        /// <param name="tenantName">
        /// The name of the tenant to be added.
        /// </param>
        /// <param name="tenantCode">
        /// The unique code assigned to the tenant.
        /// </param>
        /// <param name="tenantStatus">
        /// The status of the tenant (e.g., Active, Inactive).
        /// </param>
        /// <returns>
        /// The ID of the newly created tenant.
        /// </returns>
        public async Task<int> AddTenant(string tenantName, string tenantCode, Status tenantStatus)
        {
            return await _tenantService.AddTenant(tenantName, tenantCode, tenantStatus);
        }

        /// <summary>
        /// Adds a new tenant with the specified name and status.
        /// The tenant will be assigned a default code.
        /// </summary>
        /// <param name="tenantName">
        /// The name of the tenant to be added.
        /// </param>
        /// <param name="tenantStatus">
        /// The status of the tenant (e.g., Active, Inactive).
        /// </param>
        /// <returns>
        /// The ID of the newly created tenant.
        /// </returns>
        public async Task<int> AddTenant(string tenantName, Status tenantStatus)
        {
            return await _tenantService.AddTenant(tenantName, tenantStatus);
        }

        /// <summary>
        /// Adds a new tenant with only the specified name.
        /// The tenant will be assigned a default code and status.
        /// </summary>
        /// <param name="tenantName">
        /// The name of the tenant to be added.
        /// </param>
        /// <returns>
        /// The ID of the newly created tenant.
        /// </returns>
        public async Task<int> AddTenant(string tenantName)
        {
            return await _tenantService.AddTenant(tenantName);
        }

        /// <summary>
        /// Retrieves a list of all tenants.
        /// </summary>
        /// <returns>
        /// A list of <see cref="Tenant"/> objects representing all tenants in the system.
        /// </returns>
        public async Task<List<Tenant>> GetTenants()
        {
            return await _tenantService.GetTenants();
        }

        /// <summary>
        /// Retrieves a single tenant by their ID.
        /// </summary>
        /// <param name="tenantId">
        /// The ID of the tenant to retrieve.
        /// </param>
        /// <returns>
        /// A <see cref="Tenant"/> object representing the tenant with the specified ID.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when no tenant is found with the given ID.
        /// </exception>
        public async Task<Tenant> GetTenant(int tenantId)
        {
            return await _tenantService.GetTenant(tenantId);
        }

        /// <summary>
        /// Retrieves a single tenant by their code.
        /// </summary>
        /// <param name="tenantCode">
        /// The code of the tenant to retrieve.
        /// </param>
        /// <returns>
        /// A <see cref="Tenant"/> object representing the tenant with the specified code.
        /// </returns>
        /// <exception cref="KeyNotFoundException">
        /// Thrown when no tenant is found with the given code.
        /// </exception>
        public async Task<Tenant> GetTenant(string tenantCode)
        {
            return await _tenantService.GetTenant(tenantCode);
        }

        /// <summary>
        /// Updates the name of an existing tenant with the specified ID.
        /// </summary>
        /// <param name="tenantId">
        /// The ID of the tenant to be updated.
        /// </param>
        /// <param name="tenantName">
        /// The new name for the tenant.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public async Task UpdateTenant(int tenantId, string tenantName)
        {
            await _tenantService.UpdateTenant(tenantId, tenantName);
        }

        /// <summary>
        /// Updates the status of an existing tenant with the specified ID.
        /// </summary>
        /// <param name="tenantId">
        /// The ID of the tenant to be updated.
        /// </param>
        /// <param name="tenantStatus">
        /// The new status of the tenant (e.g., Active, Inactive).
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public async Task UpdateTenant(int tenantId, Status tenantStatus)
        {
            await _tenantService.UpdateTenant(tenantId, tenantStatus);
        }

        /// <summary>
        /// Updates the name and code of an existing tenant with the specified ID.
        /// </summary>
        /// <param name="tenantId">
        /// The ID of the tenant to be updated.
        /// </param>
        /// <param name="tenantName">
        /// The new name for the tenant.
        /// </param>
        /// <param name="tenantCode">
        /// The new code for the tenant.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public async Task UpdateTenant(int tenantId, string tenantName, string tenantCode)
        {
            await _tenantService.UpdateTenant(tenantId, tenantName, tenantCode);
        }

        /// <summary>
        /// Updates an existing tenant with the specified ID, name, code, and status.
        /// </summary>
        /// <param name="tenantId">
        /// The ID of the tenant to be updated.
        /// </param>
        /// <param name="tenantName">
        /// The new name for the tenant.
        /// </param>
        /// <param name="tenantCode">
        /// The new code for the tenant.
        /// </param>
        /// <param name="tenantStatus">
        /// The new status of the tenant (e.g., Active, Inactive).
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
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
        /// Deletes a tenant with the specified ID from the system.
        /// </summary>
        /// <param name="tenantId">
        /// The ID of the tenant to be deleted.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation.
        /// </returns>
        public async Task DeleteTenant(int tenantId)
        {
            await _tenantService.DeleteTenant(tenantId);
        }
    }
}
