namespace DbLocator.Domain;

/// <summary>
/// Represents a database.
/// </summary>
/// <param name="Id">The database user ID.</param>
/// <param name="Name">The user name.</param>
/// <param name="Database">The id of the associated database.</param>
/// <param name="Roles">List of associated roles.</param>
public class DatabaseUser(int Id, string Name, Database Database, List<DatabaseRole> Roles)
{
    /// <summary>
    /// Gets the database user ID.
    /// </summary>
    public int Id { get; init; } = Id;

    /// <summary>
    /// Gets the name of the user.
    /// </summary>
    public string Name { get; init; } = Name;

    /// <summary>
    /// Gets the associated database
    /// </summary>
    public Database Database { get; init; } = Database;

    /// <summary>
    /// Gets the list of roles for the user.
    /// </summary>
    public List<DatabaseRole> Roles { get; init; } = Roles;
}
