namespace DbLocator.Domain;

/// <summary>
/// Represents a connection to a database.
/// </summary>
/// <param name="id">The unique identifier for the connection.</param>
/// <param name="database">The identifier of the database.</param>
/// <param name="tenant">The identifier of the tenant</param>
public class Connection(int id, Database database, Tenant tenant)
{
    /// <summary>
    /// Gets the unique identifier for the connection.
    /// </summary>
    public int Id { get; init; } = id;

    /// <summary>
    /// Gets or sets the identifier of the database.
    /// </summary>
    public Database Database { get; set; } = database;

    /// <summary>
    /// Gets or sets the identifier of the tenant.
    /// </summary>
    public Tenant Tenant { get; set; } = tenant;
};
