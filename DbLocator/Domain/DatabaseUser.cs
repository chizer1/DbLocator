namespace DbLocator.Domain;

/// <summary>
/// Represents a database user within the DbLocator system, which corresponds to an
/// individual or entity that has access to one or more databases and is assigned
/// certain roles that define their permissions within those databases.
///
/// The DatabaseUser class is a crucial component in the system's security and
/// access control mechanisms. It manages the relationships between users, databases,
/// and their associated roles, ensuring that users have appropriate access levels
/// to the databases they need to work with.
///
/// This class supports complex access control scenarios where a user might need
/// different levels of access to different databases, or where multiple roles
/// need to be combined to achieve the desired permission set. The class is designed
/// to be flexible enough to handle various security requirements while maintaining
/// a clear and manageable structure.
///
/// The class is immutable by design, ensuring that user configurations remain
/// consistent throughout their lifecycle. Any modifications to a user's
/// configuration should be performed through the appropriate service layer methods.
/// </summary>
/// <param name="Id">
/// The unique identifier for the database user. This ID is used to uniquely
/// reference the user in the system and helps with database management and
/// permissions assignment. The ID is assigned by the system during creation
/// and should not be modified after the user is created.
/// </param>
/// <param name="Name">
/// The name of the user, typically representing the username or account name
/// the user uses to authenticate with the database. This can be a system username
/// or a display name for the user. The name should be descriptive and follow
/// the system's naming conventions for easy identification and management.
/// </param>
/// <param name="Databases">
/// The associated databases with which the user has access. This property links
/// the user to specific databases, defining the scope of their permissions.
/// The list can contain multiple databases, allowing the user to access different
/// databases with potentially different permission levels.
/// </param>
/// <param name="Roles">
/// A list of roles assigned to the user that define the permissions and access
/// levels the user has within the database. These roles determine what actions
/// the user can perform (e.g., read, write, admin, etc.) on the database. The
/// user can have multiple roles, and the combination of these roles defines
/// their overall permission set.
/// </param>
public class DatabaseUser(int Id, string Name, List<Database> Databases, List<DatabaseRole> Roles)
{
    /// <summary>
    /// Gets the unique identifier for the database user.
    /// This ID is used to uniquely identify the user within the system and is
    /// typically stored as the primary key for user-related records in the database.
    /// The ID is immutable and cannot be changed after the user is created.
    /// </summary>
    public int Id { get; init; } = Id;

    /// <summary>
    /// Gets the name of the user.
    /// This property holds the name (or username) of the database user, which is
    /// used for authentication and identification purposes when interacting with
    /// the database. The name should be descriptive and follow the system's naming
    /// conventions for easy identification and management. This property is immutable
    /// and cannot be changed after the user is created.
    /// </summary>
    public string Name { get; init; } = Name;

    /// <summary>
    /// Gets the associated databases.
    /// This property links the user to specific databases. It signifies that the
    /// user has access to given databases and their roles are defined within
    /// the context of those databases. The list can contain multiple databases,
    /// allowing the user to access different databases with potentially different
    /// permission levels. This property is immutable and cannot be changed after
    /// the user is created.
    /// </summary>
    public List<Database> Databases { get; init; } = Databases;

    /// <summary>
    /// Gets the list of roles assigned to the user.
    /// This list holds the roles that define the user's permissions within the
    /// database. Roles can include actions such as read-only access, write access,
    /// or administrative privileges, and the user can have multiple roles. The
    /// combination of these roles defines the user's overall permission set.
    /// This property is immutable and cannot be changed after the user is created.
    /// </summary>
    public List<DatabaseRole> Roles { get; init; } = Roles;
}
