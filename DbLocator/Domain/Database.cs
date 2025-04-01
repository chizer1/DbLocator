namespace DbLocator.Domain;

/// <summary>
/// Represents a database.
/// </summary>
/// <param name="Id">The database ID.</param>
/// <param name="Name">The name of the database.</param>
/// <param name="Type">The type of the database.</param>
/// <param name="Server">The Database server.</param>
/// <param name="Status">The status of the database.</param>
/// <param name="UseTrustedConnection"> Trusted connection.</param>
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
    /// Gets the database ID.
    /// </summary>
    public int Id { get; init; } = Id;

    /// <summary>
    /// Gets the name of the database.
    /// </summary>
    public string Name { get; init; } = Name;

    /// <summary>
    /// Gets or sets the database type.
    /// </summary>
    public DatabaseType Type { get; set; } = Type;

    /// <summary>
    /// Gets or sets the database server.
    /// </summary>
    public DatabaseServer Server { get; init; } = Server;

    /// <summary>
    /// Gets or sets the status of the database.
    /// </summary>
    public Status Status { get; set; } = Status;

    /// <summary>
    /// Gets or sets a value indicating whether to use a trusted connection.
    /// </summary>
    /// <value><c>true</c> if a trusted connection should be used; otherwise, <c>false</c>.</value>
    /// <remarks>
    /// If <c>true</c>, the database user and password are ignored.
    /// </remarks>
    public bool UseTrustedConnection { get; set; } = UseTrustedConnection;
}
