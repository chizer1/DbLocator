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
    Owner,

    /// <summary>
    /// The user can perform any activity in the database, except for modifying the database itself
    /// </summary>
    SecurityAdmin,

    /// <summary>
    /// The user can modify access to the database for other users
    /// </summary>
    AccessAdmin,

    /// <summary>
    /// The user can backup the database
    /// </summary>
    BackupOperator,

    /// <summary>
    /// The user can run any Data Definition Language (DDL) command in the database
    /// </summary>
    DdlAdmin,

    /// <summary>
    /// The user can update, delete, and insert into any table in the database. Cannot select unless used with DataReader or similar role.
    /// </summary>
    DataWriter,

    /// <summary>
    /// The user can select from any table in the database. Cannot update, delete, or insert unless used with DataWriter or similar role.
    /// </summary>
    DataReader,

    /// <summary>
    /// The user cannot update, delete, or insert into any table in the database.
    /// </summary>
    DenyDataWriter,

    /// <summary>
    /// The user cannot select from any table in the database.
    /// </summary>
    DenyDataReader
}
