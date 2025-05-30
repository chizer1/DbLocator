namespace DbLocator.Domain;

/// <summary>
/// Represents a database server that hosts one or more databases.
/// This class contains essential details about the server, such as its name,
/// IP address, and domain name, and is used for managing the server information
/// in a system that locates and connects to different database servers.
/// It is also useful in identifying whether a given server is linked to the
/// primary database system for operations like DbLocator.
/// </summary>
/// <param name="Id">
/// The unique identifier for the database server. This ID is used to uniquely
/// reference this server within a system or database and typically acts as the
/// primary key in any associated records.
/// </param>
/// <param name="Name">
/// The friendly or logical name assigned to the database server. This name is
/// typically used when referencing the server in configurations, logs, or
/// connection strings.
/// </param>
/// <param name="IpAddress">
/// The IP address of the database server. This address is essential for establishing
/// a connection to the server over a network and is part of the network configuration.
/// </param>
/// <param name="HostName">
/// The host name of the database server. This is the machine name or identifier
/// that is used in the network to reference the server, often used for DNS resolution.
/// </param>
/// <param name="FullyQualifiedDomainName">
/// The fully qualified domain name (FQDN) of the database server, which includes
/// the hostname and the domain name. This is useful for connecting to the server
/// in more complex network environments, such as those that require DNS resolution.
/// </param>
/// <param name="IsLinkedServer">
/// A boolean value indicating whether this server is a linked server.
/// If <c>true</c>, this server is linked to the DbLocator database, meaning it
/// is part of the system’s infrastructure for locating and managing databases
/// in a distributed environment. Otherwise, <c>false</c>.
/// </param>
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
    /// Gets the unique identifier for the database server.
    /// This ID uniquely identifies the server in the system and is used for
    /// referencing the server in other database records or configurations.
    /// </summary>
    public int Id { get; init; } = Id;

    /// <summary>
    /// Gets the friendly name of the database server.
    /// This name is typically a user-friendly label used to identify the
    /// server in logs, connection strings, and UI representations.
    /// </summary>
    public string Name { get; init; } = Name;

    /// <summary>
    /// Gets the IP address of the database server.
    /// This address is used for network-level communication to establish a
    /// connection to the server, typically within a local area network (LAN)
    /// or wide area network (WAN).
    /// </summary>
    public string IpAddress { get; init; } = IpAddress;

    /// <summary>
    /// Gets the host name of the database server.
    /// The host name is the machine name or identifier used to refer to the
    /// server on the network. It is often used for DNS resolution when
    /// connecting to the server.
    /// </summary>
    public string HostName { get; init; } = HostName;

    /// <summary>
    /// Gets the fully qualified domain name (FQDN) of the database server.
    /// The FQDN combines the host name and domain name, allowing it to be
    /// resolvable over a network using DNS.
    /// </summary>
    public string FullyQualifiedDomainName { get; init; } = FullyQualifiedDomainName;

    /// <summary>
    /// Gets a value indicating whether this server is linked to the DbLocator database.
    /// A linked server is typically a part of a broader system configuration where
    /// the server is part of a multi-server architecture, usually for operations like
    /// location resolution, failover, or load balancing in the context of database management.
    /// </summary>
    public bool IsLinkedServer { get; init; } = IsLinkedServer;
}
