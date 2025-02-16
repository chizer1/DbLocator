namespace DbLocator.Domain;

/// <summary>
/// Represents a connection to a database.
/// </summary>
/// <param name="id">The unique identifier for the connection.</param>
/// <param name="database">The database for the connection.</param>
/// <param name="tenant">The tenant for the connection.</param>
public class Connection(int id, Database database, Tenant tenant)
{
    /// <summary>
    /// The unique identifier for the connection.
    /// </summary>
    public int Id { get; set; } = id;

    /// <summary>
    /// The database for the connection.
    /// </summary>
    public Database Database { get; set; } = database;

    /// <summary>
    /// The tenant for the connection.
    /// </summary>
    public Tenant Tenant { get; set; } = tenant;
};
