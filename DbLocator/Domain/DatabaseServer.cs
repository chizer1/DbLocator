namespace DbLocator.Domain;

/// <summary>
/// Represents a database server.
/// </summary>
/// <param name="id">The unique identifier of the database server.</param>
/// <param name="name">The name of the database server.</param>
/// <param name="ipAddress">The IP address of the database server.</param>
/// <param name="hostName">The host name of the database server.</param>
/// <param name="fullyQualifiedDomainName">The fully qualified domain name of the database server.</param>
/// <param name="isLinkedServer">This server is linked to where DbLocator database lives</param>
public class DatabaseServer(
    int id,
    string name,
    string ipAddress,
    string hostName,
    string fullyQualifiedDomainName,
    bool isLinkedServer
)
{
    /// <summary>
    /// Gets the unique identifier of the database server.
    /// </summary>
    public int Id { get; init; } = id;

    /// <summary>
    /// Gets the friendly name of the database server.
    /// </summary>
    public string Name { get; init; } = name;

    /// <summary>
    /// Gets the IP address of the database server.
    /// </summary>
    public string IpAddress { get; init; } = ipAddress;

    /// <summary>
    /// Gets the host name of the database server.
    /// </summary>
    public string HostName { get; init; } = hostName;

    /// <summary>
    /// Gets the fully qualified domain name of the database server.
    /// </summary>
    public string FullyQualifiedDomainName { get; init; } = fullyQualifiedDomainName;

    /// <summary>
    /// This server is linked to where DbLocator database lives
    /// </summary>
    public bool IsLinkedServer { get; init; } = isLinkedServer;
};
