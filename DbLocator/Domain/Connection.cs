namespace DbLocator.Domain;

/// <summary>
/// Represents a connection to a database.
/// </summary>
/// <param name="Id">The unique identifier for the connection.</param>
/// <param name="Database">The database for the connection.</param>
/// <param name="Tenant">The tenant for the connection.</param>
public class Connection(int Id, Database Database, Tenant Tenant)
{
    /// <summary>
    /// The unique identifier for the connection.
    /// </summary>
    public int Id { get; set; } = Id;

    /// <summary>
    /// The database for the connection.
    /// </summary>
    public Database Database { get; set; } = Database;

    /// <summary>
    /// The tenant for the connection.
    /// </summary>
    public Tenant Tenant { get; set; } = Tenant;
};
