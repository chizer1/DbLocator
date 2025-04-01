namespace DbLocator.Domain;

/// <summary>
/// Represents a database server.
/// </summary>
/// <param name="Id">The unique identifier of the database server.</param>
/// <param name="Name">The name of the database server.</param>
/// <param name="IpAddress">The IP address of the database server.</param>
/// <param name="HostName">The host name of the database server.</param>
/// <param name="FullyQualifiedDomainName">The fully qualified domain name of the database server.</param>
/// <param name="IsLinkedServer">This server is linked to where DbLocator database lives</param>
public class DatabaseServer(
    int Id,
    string Name,
    string IpAddress,
    string HostName,
    string FullyQualifiedDomainName,
    bool IsLinkedServer
)
{
    /// <summary>
    /// Gets the unique identifier of the database server.
    /// </summary>
    public int Id { get; init; } = Id;

    /// <summary>
    /// Gets the friendly name of the database server.
    /// </summary>
    public string Name { get; init; } = Name;

    /// <summary>
    /// Gets the IP address of the database server.
    /// </summary>
    public string IpAddress { get; init; } = IpAddress;

    /// <summary>
    /// Gets the host name of the database server.
    /// </summary>
    public string HostName { get; init; } = HostName;

    /// <summary>
    /// Gets the fully qualified domain name of the database server.
    /// </summary>
    public string FullyQualifiedDomainName { get; init; } = FullyQualifiedDomainName;

    /// <summary>
    /// This server is linked to where DbLocator database lives
    /// </summary>
    public bool IsLinkedServer { get; init; } = IsLinkedServer;
};
