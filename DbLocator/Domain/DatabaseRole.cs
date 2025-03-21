namespace DbLocator.Domain;

/// <summary>
/// Defines the roles that can be assigned to a user in the database.
/// These roles directly map to the Database roles in SQL Server.
/// For more information, see:
/// https://learn.microsoft.com/en-us/sql/relational-databases/security/authentication-access/database-level-roles?view=sql-server-ver16#fixed-database-roles
/// </summary>
public enum DatabaseRole
{
    /// <summary>
    /// The user has all database permissions
    /// </summary>
    Owner = 1,

    /// <summary>
    /// The user can perform any activity in the database, except for modifying the database itself
    /// </summary>
    SecurityAdmin = 2,

    /// <summary>
    /// The user can modify access to the database for other users
    /// </summary>
    AccessAdmin = 3,

    /// <summary>
    /// The user can backup the database
    /// </summary>
    BackupOperator = 4,

    /// <summary>
    /// The user can run any Data Definition Language (DDL) command in the database
    /// </summary>
    DdlAdmin = 5,

    /// <summary>
    /// The user can update, delete, and insert into any table in the database. Cannot select unless used with DataReader or similar role.
    /// </summary>
    DataWriter = 6,

    /// <summary>
    /// The user can select from any table in the database. Cannot update, delete, or insert unless used with DataWriter or similar role.
    /// </summary>
    DataReader = 7,

    /// <summary>
    /// The user cannot update, delete, or insert into any table in the database.
    /// </summary>
    DenyDataWriter = 8,

    /// <summary>
    /// The user cannot select from any table in the database.
    /// </summary>
    DenyDataReader = 9
}
