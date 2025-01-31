namespace DbLocator.Domain;

/// <summary>
/// Represents a type of database.
/// </summary>
/// <param name="id">The unique identifier of the database type.</param>
/// <param name="name">The name of the database type.</param>
public class DatabaseType(int id, string name)
{
    /// <summary>
    /// Gets the unique identifier of the database type.
    /// </summary>
    public int Id { get; init; } = id;

    /// <summary>
    /// Gets the name of the database type.
    /// </summary>
    public string Name { get; init; } = name;
}
