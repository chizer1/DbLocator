namespace DbLocator.Domain
{
    /// <summary>
    /// Represents a database with various properties that define its characteristics,
    /// configuration, and operational status. This class is used to encapsulate details
    /// about a specific database, such as its name, type, server, and connection settings.
    /// It is primarily used in scenarios where multiple databases need to be tracked
    /// and managed within a system, such as in multi-tenant applications or environments
    /// that handle multiple data sources.
    /// </summary>
    /// <param name="Id">
    /// The unique identifier for the database. This ID is used to distinguish this
    /// database instance from others within the system. It is typically used as
    /// a primary key in a database.
    /// </param>
    /// <param name="Name">
    /// The name of the database. This is the logical name that represents the database
    /// and is typically used when referencing it in connection strings or queries.
    /// </param>
    /// <param name="Type">
    /// The <see cref="DatabaseType"/> that represents the functional purpose of the database within the system.
    /// This allows tenants to have multiple databases, each serving distinct roles or functionalities.
    /// </param>
    /// <param name="Server">
    /// The <see cref="DatabaseServer"/> object that represents the server on which
    /// the database is hosted. This includes details like the server's hostname or IP address
    /// and any additional configuration required to connect to it.
    /// </param>
    /// <param name="Status">
    /// The <see cref="Status"/> that represents the operational state of the database.
    /// It could indicate whether the database is online, offline, in maintenance mode, etc.
    /// </param>
    /// <param name="UseTrustedConnection">
    /// A boolean flag indicating whether a trusted connection should be used to access the database.
    /// If set to <c>true</c>, the system will use the Windows authentication of the current user
    /// for the database connection, ignoring the need for a username and password.
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
        /// database records within the system.
        /// </summary>
        public int Id { get; init; } = Id;

        /// <summary>
        /// Gets the name of the database.
        /// This is the logical name assigned to the database, typically used in
        /// database management tools, connection strings, and queries.
        /// </summary>
        public string Name { get; init; } = Name;

        /// <summary>
        /// Gets or sets the type of the database.
        /// This property specifies the type of the database (e.g., SQL Server, MySQL, etc.)
        /// and can be used to configure system behavior based on the specific database engine.
        /// </summary>
        public DatabaseType Type { get; set; } = Type;

        /// <summary>
        /// Gets the database server associated with this database.
        /// This property represents the server (such as its IP address or hostname)
        /// where the database is hosted and includes details required to establish a connection.
        /// </summary>
        public DatabaseServer Server { get; init; } = Server;

        /// <summary>
        /// Gets or sets the current status of the database.
        /// This status indicates whether the database is active or inactive.
        /// </summary>
        public Status Status { get; set; } = Status;

        /// <summary>
        /// Gets or sets a value indicating whether a trusted connection should be used when connecting to the database.
        /// </summary>
        /// <value><c>true</c> if a trusted connection (using Windows authentication) should be used; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// When set to <c>true</c>, the system will bypass the need for a username and password, relying
        /// on the credentials of the current Windows user for authentication with the database.
        /// </remarks>
        public bool UseTrustedConnection { get; set; } = UseTrustedConnection;
    }
}
