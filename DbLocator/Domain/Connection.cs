namespace DbLocator.Domain;

/// <summary>
/// Represents a logical connection between a tenant and a specific database instance.
/// This model is used to track which tenant is associated with which database,
/// enabling dynamic resolution of data sources in a multi-tenant architecture.
/// </summary>
/// <param name="Id">
/// The unique numeric identifier assigned to this connection. This value is typically
/// used as the primary key in a data store and allows referencing and managing
/// connection records efficiently.
/// </param>
/// <param name="Database">
/// The <see cref="Database"/> object that contains metadata and connection details
/// for the physical or logical database this connection is associated with.
/// </param>
/// <param name="Tenant">
/// The <see cref="Tenant"/> object that represents the tenant entity, such as a customer
/// or client, which is assigned to the specified database through this connection.
/// </param>
public class Connection(int Id, Database Database, Tenant Tenant)
{
    /// <summary>
    /// Gets or sets the unique identifier for this connection.
    /// This ID is typically auto-generated and serves as a key to identify and distinguish
    /// individual connection entries within the system.
    /// </summary>
    public int Id { get; set; } = Id;

    /// <summary>
    /// Gets or sets the database instance associated with this connection.
    /// This property encapsulates all necessary details about the target database,
    /// such as its name, host, and other metadata required to connect to it.
    /// </summary>
    public Database Database { get; set; } = Database;

    /// <summary>
    /// Gets or sets the tenant associated with this connection.
    /// The tenant represents a consumer or organization whose data resides in the
    /// specified database. This mapping allows the system to resolve and route data
    /// requests appropriately in a multi-tenant environment.
    /// </summary>
    public Tenant Tenant { get; set; } = Tenant;
}
