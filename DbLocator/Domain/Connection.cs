namespace DbLocator.Domain;

/// <summary>
/// Represents a logical connection between a tenant and a specific database instance
/// within the DbLocator system. This class serves as a crucial component in the
/// multi-tenant architecture, enabling dynamic resolution of data sources and
/// managing the relationships between tenants and their associated databases.
///
/// The Connection class establishes a many-to-many relationship between tenants
/// and databases, allowing a single tenant to access multiple databases and a
/// single database to serve multiple tenants. This flexibility is essential for
/// complex multi-tenant scenarios where data isolation and resource sharing need
/// to be carefully managed.
///
/// This class is designed to support various database access patterns and
/// configurations, making it suitable for use in both simple and complex
/// multi-tenant architectures. It includes support for different database types,
/// server configurations, and tenant-specific access controls.
///
/// The class is mutable by design, allowing for dynamic updates to the connection
/// configuration as tenant requirements or database configurations change over time.
/// </summary>
/// <param name="Id">
/// The unique numeric identifier assigned to this connection. This value is typically
/// used as the primary key in a data store and allows referencing and managing
/// connection records efficiently. The ID is assigned by the system during creation
/// and should not be modified after the connection is created.
/// </param>
/// <param name="Database">
/// The <see cref="Database"/> object that contains metadata and connection details
/// for the physical or logical database this connection is associated with. This
/// includes information about the database's server, type, and configuration that
/// is essential for establishing and maintaining the connection.
/// </param>
/// <param name="Tenant">
/// The <see cref="Tenant"/> object that represents the tenant entity, such as a customer
/// or client, which is assigned to the specified database through this connection.
/// The tenant information is crucial for managing access controls and ensuring
/// proper data isolation between different tenants.
/// </param>
public class Connection(int Id, Database Database, Tenant Tenant)
{
    /// <summary>
    /// Gets or sets the unique identifier for this connection.
    /// This ID is typically auto-generated and serves as a key to identify and distinguish
    /// individual connection entries within the system. The ID is immutable and cannot
    /// be changed after the connection is created.
    /// </summary>
    public int Id { get; set; } = Id;

    /// <summary>
    /// Gets or sets the database instance associated with this connection.
    /// This property encapsulates all necessary details about the target database,
    /// such as its name, host, and other metadata required to connect to it. The
    /// database information is crucial for establishing and maintaining the connection
    /// between the tenant and the database. This property can be modified after the
    /// connection is created to reflect changes in the database configuration or
    /// to reassign the connection to a different database.
    /// </summary>
    public Database Database { get; set; } = Database;

    /// <summary>
    /// Gets or sets the tenant associated with this connection.
    /// The tenant represents a consumer or organization whose data resides in the
    /// specified database. This mapping allows the system to resolve and route data
    /// requests appropriately in a multi-tenant environment. The tenant information
    /// is essential for managing access controls and ensuring proper data isolation
    /// between different tenants. This property can be modified after the connection
    /// is created to reflect changes in the tenant's database access requirements
    /// or to reassign the connection to a different tenant.
    /// </summary>
    public Tenant Tenant { get; set; } = Tenant;
}
