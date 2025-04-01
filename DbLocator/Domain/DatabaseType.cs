namespace DbLocator.Domain;

/// <summary>
/// Represents a type of database.
/// </summary>
/// <param name="Id">The unique identifier of the database type.</param>
/// <param name="Name">The name of the database type.</param>
public class DatabaseType(int Id, string Name)
{
    /// <summary>
    /// Gets the unique identifier of the database type.
    /// </summary>
    public int Id { get; init; } = Id;

    /// <summary>
    /// Gets the name of the database type.
    /// </summary>
    public string Name { get; init; } = Name;
}
