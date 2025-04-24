namespace DbLocator.Domain;

/// <summary>
/// Represents a role assigned to a database user.
/// </summary>
/// <param name="Id">The unique identifier for the database user role.</param>
/// <param name="UserName">The name of the user this role is assigned to.</param>
/// <param name="Role">The role assigned to the user.</param>
public class DatabaseUserRole(int Id, string UserName, DatabaseRole Role)
{
    /// <summary>
    /// Gets the unique identifier for the database user role.
    /// </summary>
    public int Id { get; init; } = Id;

    /// <summary>
    /// Gets the name of the user this role is assigned to.
    /// </summary>
    public string UserName { get; init; } = UserName;

    /// <summary>
    /// Gets the role assigned to the user.
    /// </summary>
    public DatabaseRole Role { get; init; } = Role;
}
