namespace DbLocator.Domain
{
    /// <summary>
    /// Represents a database user, which corresponds to an individual or entity
    /// that has access to a database and is assigned certain roles that define their
    /// permissions within that database. The user can be associated with a specific
    /// database and have multiple roles that dictate what actions they are authorized
    /// to perform.
    /// </summary>
    /// <param name="Id">
    /// The unique identifier for the database user. This ID is used to uniquely
    /// reference the user in the system and helps with database management and
    /// permissions assignment.
    /// </param>
    /// <param name="Name">
    /// The name of the user, typically representing the username or account name
    /// the user uses to authenticate with the database. This can be a system username
    /// or a display name for the user.
    /// </param>
    /// <param name="Database">
    /// The associated database with which the user has access. This property links
    /// the user to a specific database, defining the scope of their permissions.
    /// </param>
    /// <param name="Roles">
    /// A list of roles assigned to the user that define the permissions and access
    /// levels the user has within the database. These roles determine what actions
    /// the user can perform (e.g., read, write, admin, etc.) on the database.
    /// </param>
    public class DatabaseUser(int Id, string Name, Database Database, List<DatabaseRole> Roles)
    {
        /// <summary>
        /// Gets the unique identifier for the database user.
        /// This ID is used to uniquely identify the user within the system and is
        /// typically stored as the primary key for user-related records in the database.
        /// </summary>
        public int Id { get; init; } = Id;

        /// <summary>
        /// Gets the name of the user.
        /// This property holds the name (or username) of the database user, which is
        /// used for authentication and identification purposes when interacting with
        /// the database.
        /// </summary>
        public string Name { get; init; } = Name;

        /// <summary>
        /// Gets the associated database.
        /// This property links the user to a specific database. It signifies that the
        /// user has access to the given database and their roles are defined within
        /// the context of that database.
        /// </summary>
        public Database Database { get; init; } = Database;

        /// <summary>
        /// Gets the list of roles assigned to the user.
        /// This list holds the roles that define the user's permissions within the
        /// database. Roles can include actions such as read-only access, write access,
        /// or administrative privileges, and the user can have multiple roles.
        /// </summary>
        public List<DatabaseRole> Roles { get; init; } = Roles;
    }
}
