namespace DbLocator.Domain;

/// <summary>
/// Represents a database server.
/// </summary>
/// <param name="id">The unique identifier of the database server.</param>
/// <param name="name">The name of the database server.</param>
/// <param name="ipAddress">The IP address of the database server.</param>
public class DatabaseServer(int id, string name, string ipAddress)
{
    /// <summary>
    /// Gets the unique identifier of the database server.
    /// </summary>
    public int Id { get; init; } = id;

    /// <summary>
    /// Gets the name of the database server.
    /// </summary>
    public string Name { get; init; } = name;

    /// <summary>
    /// Gets the IP address of the database server.
    /// </summary>
    public string IpAddress { get; init; } = ipAddress;
};
