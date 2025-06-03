namespace DbLocator.Domain;

/// <summary>
/// Represents a database server that hosts one or more databases within the DbLocator system.
/// This class encapsulates all essential information about a database server, including its
/// network configuration, identification details, and operational status. It serves as a
/// central component for managing database server resources and their relationships with
/// databases in a distributed environment.
///
/// The DatabaseServer class is designed to support various server identification methods,
/// allowing flexible server management through hostnames, fully qualified domain names (FQDNs),
/// or IP addresses. This flexibility is particularly useful in complex network environments
/// where different identification methods may be required for different purposes.
///
/// This class is immutable by design, ensuring that server configurations remain consistent
/// throughout their lifecycle. Any modifications to a server's configuration should be
/// performed through the appropriate service layer methods.
/// </summary>
/// <param name="Id">
/// The unique identifier for the database server. This ID is used to uniquely
/// reference this server within the system and typically acts as the primary key
/// in any associated records. The ID is assigned by the system and should not be
/// modified after creation.
/// </param>
/// <param name="Name">
/// The friendly or logical name assigned to the database server. This name is
/// typically used when referencing the server in configurations, logs, or
/// connection strings. The name should be descriptive and follow the system's
/// naming conventions for easy identification and management.
/// </param>
/// <param name="IpAddress">
/// The IP address of the database server. This address is essential for establishing
/// a connection to the server over a network and is part of the network configuration.
/// The IP address can be either IPv4 or IPv6 format, depending on the network setup.
/// </param>
/// <param name="HostName">
/// The host name of the database server. This is the machine name or identifier
/// that is used in the network to reference the server, often used for DNS resolution.
/// The host name is typically a short name that identifies the server within a local
/// network environment.
/// </param>
/// <param name="FullyQualifiedDomainName">
/// The fully qualified domain name (FQDN) of the database server, which includes
/// the hostname and the domain name. This is useful for connecting to the server
/// in more complex network environments, such as those that require DNS resolution
/// across different domains or subdomains. The FQDN provides a globally unique
/// identifier for the server within the domain name system.
/// </param>
/// <param name="IsLinkedServer">
/// A boolean value indicating whether this server is a linked server in the context
/// of the DbLocator system. If <c>true</c>, this server is linked to the DbLocator
/// database, meaning it is part of the system's infrastructure for locating and
/// managing databases in a distributed environment. Linked servers are typically
/// used in scenarios where direct access to the server is not possible or when
/// the server is part of a more complex database architecture.
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
    /// The ID is immutable and cannot be changed after the server is created.
    /// </summary>
    public int Id { get; init; } = Id;

    /// <summary>
    /// Gets the friendly name of the database server.
    /// This name is typically a user-friendly label used to identify the
    /// server in logs, connection strings, and UI representations. The name
    /// should be descriptive and follow the system's naming conventions for
    /// easy identification and management. This property is immutable and
    /// cannot be changed after the server is created.
    /// </summary>
    public string Name { get; init; } = Name;

    /// <summary>
    /// Gets the IP address of the database server.
    /// This address is used for network-level communication to establish a
    /// connection to the server, typically within a local area network (LAN)
    /// or wide area network (WAN). The IP address can be either IPv4 or IPv6
    /// format, depending on the network setup. This property is immutable and
    /// cannot be changed after the server is created.
    /// </summary>
    public string IpAddress { get; init; } = IpAddress;

    /// <summary>
    /// Gets the host name of the database server.
    /// The host name is the machine name or identifier used to refer to the
    /// server on the network. It is often used for DNS resolution when
    /// connecting to the server. The host name is typically a short name that
    /// identifies the server within a local network environment. This property
    /// is immutable and cannot be changed after the server is created.
    /// </summary>
    public string HostName { get; init; } = HostName;

    /// <summary>
    /// Gets the fully qualified domain name (FQDN) of the database server.
    /// The FQDN combines the host name and domain name, allowing it to be
    /// resolvable over a network using DNS. This is particularly useful in
    /// complex network environments where different identification methods
    /// may be required for different purposes. The FQDN provides a globally
    /// unique identifier for the server within the domain name system. This
    /// property is immutable and cannot be changed after the server is created.
    /// </summary>
    public string FullyQualifiedDomainName { get; init; } = FullyQualifiedDomainName;

    /// <summary>
    /// Gets a value indicating whether this server is linked to the DbLocator database.
    /// A linked server is typically a part of a broader system configuration where
    /// the server is part of a multi-server architecture, usually for operations like
    /// location resolution, failover, or load balancing in the context of database
    /// management. Linked servers are used when direct access to the server is not
    /// possible or when the server is part of a more complex database architecture.
    /// This property is immutable and cannot be changed after the server is created.
    /// </summary>
    public bool IsLinkedServer { get; init; } = IsLinkedServer;
}
