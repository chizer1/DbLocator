namespace DbLocator.Domain;

/// <summary>
/// Represents a database with various properties that define its characteristics,
/// configuration, and operational status within the DbLocator system. This class
/// serves as a comprehensive model for managing database instances across different
/// servers and environments.
///
/// The Database class encapsulates all essential information needed to identify,
/// locate, and connect to a specific database instance. It maintains relationships
/// with other domain entities such as DatabaseServer and DatabaseType, providing
/// a complete picture of the database's context within the system.
///
/// This class is designed to support various database configurations and operational
/// states, making it suitable for use in both simple and complex database management
/// scenarios. It includes support for different authentication methods, server
/// configurations, and operational statuses.
///
/// The class is immutable by design, ensuring that database configurations remain
/// consistent throughout their lifecycle. Any modifications to a database's
/// configuration should be performed through the appropriate service layer methods.
/// </summary>
/// <param name="Id">
/// The unique identifier for the database. This ID is used to distinguish this
/// database instance from others within the system. It is typically used as a
/// primary key in a database and is assigned by the system during creation.
/// The ID should not be modified after the database is created.
/// </param>
/// <param name="Name">
/// The name of the database. This is the logical name that represents the database
/// and is typically used when referencing it in connection strings or queries.
/// The name should follow the system's naming conventions and be descriptive of
/// the database's purpose or contents.
/// </param>
/// <param name="Type">
/// The <see cref="DatabaseType"/> that represents the functional purpose of the
/// database within the system. This allows tenants to have multiple databases,
/// each serving distinct roles or functionalities. The type helps categorize
/// databases based on their intended use, such as operational data, analytical
/// data, or archival data.
/// </param>
/// <param name="Server">
/// The <see cref="DatabaseServer"/> object that represents the server on which
/// the database is hosted. This includes details like the server's hostname or
/// IP address and any additional configuration required to connect to it. The
/// server information is crucial for establishing connections to the database
/// and managing its physical location within the infrastructure.
/// </param>
/// <param name="Status">
/// The <see cref="Status"/> that represents the operational state of the database.
/// This status indicates whether the database is currently active and available
/// for use, or if it is in an inactive state. The status is used to manage
/// database availability and access control within the system.
/// </param>
/// <param name="UseTrustedConnection">
/// A boolean flag indicating whether a trusted connection should be used to access
/// the database. If set to <c>true</c>, the system will use the Windows authentication
/// of the current user for the database connection, ignoring the need for a username
/// and password. This is particularly useful in Windows environments where integrated
/// security is preferred over SQL Server authentication.
/// </param>
public class Database(
    int Id,
    string Name,
    DatabaseType Type,
    DatabaseServer Server,
    Status Status,
    bool UseTrustedConnection
)
{
    /// <summary>
    /// Gets the unique identifier for the database.
    /// This ID serves as the primary key and is used for identifying and managing
    /// database records within the system. The ID is immutable and cannot be
    /// changed after the database is created.
    /// </summary>
    public int Id { get; init; } = Id;

    /// <summary>
    /// Gets the name of the database.
    /// This is the logical name assigned to the database, typically used in
    /// database management tools, connection strings, and queries. The name
    /// should be descriptive and follow the system's naming conventions for
    /// easy identification and management. This property is immutable and
    /// cannot be changed after the database is created.
    /// </summary>
    public string Name { get; init; } = Name;

    /// <summary>
    /// Gets or sets the type of the database.
    /// This property specifies the type of the database (e.g., SQL Server, MySQL, etc.)
    /// and can be used to configure system behavior based on the specific database engine.
    /// The type helps categorize databases based on their intended use and determines
    /// how the system interacts with the database. This property can be modified
    /// after the database is created to reflect changes in the database's purpose
    /// or role within the system.
    /// </summary>
    public DatabaseType Type { get; set; } = Type;

    /// <summary>
    /// Gets the database server associated with this database.
    /// This property represents the server (such as its IP address or hostname)
    /// where the database is hosted and includes details required to establish
    /// a connection. The server information is crucial for managing the database's
    /// physical location and network configuration. This property is immutable
    /// and cannot be changed after the database is created.
    /// </summary>
    public DatabaseServer Server { get; init; } = Server;

    /// <summary>
    /// Gets or sets the current status of the database.
    /// This status indicates whether the database is active or inactive, and
    /// is used to manage database availability and access control within the
    /// system. The status can be modified after the database is created to
    /// reflect changes in the database's operational state.
    /// </summary>
    public Status Status { get; set; } = Status;

    /// <summary>
    /// Gets or sets a value indicating whether a trusted connection should be used
    /// when connecting to the database.
    /// </summary>
    /// <value>
    /// <c>true</c> if a trusted connection (using Windows authentication) should be used;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// When set to <c>true</c>, the system will bypass the need for a username and
    /// password, relying on the credentials of the current Windows user for
    /// authentication with the database. This is particularly useful in Windows
    /// environments where integrated security is preferred over SQL Server
    /// authentication. The setting can be modified after the database is created
    /// to reflect changes in the authentication requirements.
    /// </remarks>
    public bool UseTrustedConnection { get; set; } = UseTrustedConnection;
}
