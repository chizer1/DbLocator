namespace DbLocator.Domain;

/// <summary>
/// Represents a database.
/// </summary>
/// <param name="id">The database ID.</param>
/// <param name="name">The name of the database.</param>
/// <param name="databaseType">The type of the database.</param>
/// <param name="databaseServer">The Database server.</param>
/// <param name="status">The status of the database.</param>
/// <param name="useTrustedConnection"> Use trusted connection.</param>
public class Database(
    int id,
    string name,
    DatabaseType databaseType,
    DatabaseServer databaseServer,
    Status status,
    bool useTrustedConnection
)
{
    /// <summary>
    /// Gets the database ID.
    /// </summary>
    public int Id { get; init; } = id;

    /// <summary>
    /// Gets the name of the database.
    /// </summary>
    public string Name { get; init; } = name;

    /// <summary>
    /// Gets or sets the database type.
    /// </summary>
    public DatabaseType Type { get; set; } = databaseType;

    /// <summary>
    /// Gets the ID of the database server.
    /// </summary>
    public DatabaseServer Server { get; init; } = databaseServer;

    /// <summary>
    /// Gets or sets the status of the database.
    /// </summary>
    public Status Status { get; set; } = status;

    /// <summary>
    /// Gets or sets a value indicating whether to use a trusted connection.
    /// </summary>
    /// <value><c>true</c> if a trusted connection should be used; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// If <c>true</c>, the database user and password are ignored.
    /// </remarks>
    public bool UseTrustedConnection { get; set; } = useTrustedConnection;
}
