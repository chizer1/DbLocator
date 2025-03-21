namespace DbLocator.Domain;

/// <summary>
/// Represents a database.
/// </summary>
/// <param name="id">The database user ID.</param>
/// <param name="name">The user name.</param>
/// <param name="databaseId">The id of the associated database.</param>
/// <param name="roles">List of associated roles.</param>
public class DatabaseUser(int id, string name, int databaseId, IEnumerable<DatabaseRole> roles)
{
    /// <summary>
    /// Gets the database user ID.
    /// </summary>
    public int Id { get; init; } = id;

    /// <summary>
    /// Gets the name of the user.
    /// </summary>
    public string Name { get; init; } = name;

    /// <summary>
    /// Gets the associated database id.
    /// </summary>
    public int DatabaseId { get; init; } = databaseId;

    /// <summary>
    /// Gets the list of roles for the user.
    /// </summary>
    public IEnumerable<DatabaseRole> Roles { get; init; } = roles;
}
