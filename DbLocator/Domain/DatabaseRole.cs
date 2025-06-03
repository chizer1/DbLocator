namespace DbLocator.Domain;

/// <summary>
/// Defines the roles that can be assigned to a user in a SQL Server database.
/// These roles directly map to the fixed database roles in SQL Server and provide
/// a standardized way to manage database-level permissions. Each role represents
/// a predefined set of permissions that can be granted to database users.
///
/// The roles in this enum are based on SQL Server's fixed database roles, which
/// are predefined roles that exist in every database. These roles provide a
/// convenient way to manage common database-level permissions without having to
/// create and manage custom roles.
///
/// For more detailed information about these roles and their permissions, see:
/// https://learn.microsoft.com/en-us/sql/relational-databases/security/authentication-access/database-level-roles?view=sql-server-ver16#fixed-database-roles
/// </summary>
public enum DatabaseRole
{
    /// <summary>
    /// The database owner role (db_owner).
    /// Users with this role have full control over the database and can perform
    /// any activity in the database. This includes:
    /// - Creating, modifying, and dropping database objects
    /// - Managing database security
    /// - Backing up and restoring the database
    /// - Managing database configuration
    /// - Adding or removing users from other database roles
    ///
    /// This is the most powerful role in the database and should be assigned
    /// with caution, typically only to database administrators.
    /// </summary>
    Owner = 1,

    /// <summary>
    /// The security administrator role (db_securityadmin).
    /// Users with this role can manage database security settings and permissions.
    /// This includes:
    /// - Managing database users and roles
    /// - Granting and revoking permissions
    /// - Managing database-level security policies
    /// - Creating and managing database-level certificates and keys
    ///
    /// This role is suitable for security administrators who need to manage
    /// database security but don't need full database ownership privileges.
    /// </summary>
    SecurityAdmin = 2,

    /// <summary>
    /// The access administrator role (db_accessadmin).
    /// Users with this role can manage database access by:
    /// - Adding or removing database users
    /// - Adding or removing database roles
    /// - Managing Windows groups and SQL Server logins in the database
    ///
    /// This role is useful for administrators who need to manage database access
    /// but don't need to manage security settings or database objects.
    /// </summary>
    AccessAdmin = 3,

    /// <summary>
    /// The backup operator role (db_backupoperator).
    /// Users with this role can perform database backup operations, including:
    /// - Creating database backups
    /// - Creating differential backups
    /// - Creating transaction log backups
    /// - Restoring backups (if they have the necessary permissions)
    ///
    /// This role is typically assigned to users responsible for database backup
    /// and recovery operations.
    /// </summary>
    BackupOperator = 4,

    /// <summary>
    /// The DDL administrator role (db_ddladmin).
    /// Users with this role can run any Data Definition Language (DDL) command
    /// in the database, including:
    /// - Creating, modifying, and dropping database objects (tables, views, etc.)
    /// - Creating and managing indexes
    /// - Creating and managing constraints
    /// - Creating and managing stored procedures and functions
    ///
    /// This role is suitable for database developers and administrators who need
    /// to manage database schema but don't need full database ownership privileges.
    /// </summary>
    DdlAdmin = 5,

    /// <summary>
    /// The data writer role (db_datawriter).
    /// Users with this role can modify data in any user table in the database,
    /// including:
    /// - Inserting new records
    /// - Updating existing records
    /// - Deleting records
    ///
    /// Note: This role does not include SELECT permissions. Users will need
    /// the DataReader role or explicit SELECT permissions to read data.
    ///
    /// This role is suitable for users who need to modify data but don't need
    /// to read data or manage database objects.
    /// </summary>
    DataWriter = 6,

    /// <summary>
    /// The data reader role (db_datareader).
    /// Users with this role can read data from any user table in the database,
    /// including:
    /// - Selecting data from tables
    /// - Viewing table contents
    /// - Reading data through views
    ///
    /// Note: This role does not include permissions to modify data. Users will
    /// need the DataWriter role or explicit INSERT/UPDATE/DELETE permissions
    /// to modify data.
    ///
    /// This role is suitable for users who need to read data but don't need
    /// to modify data or manage database objects.
    /// </summary>
    DataReader = 7,

    /// <summary>
    /// The deny data writer role.
    /// This is a custom role that explicitly denies data modification permissions
    /// to users. When assigned, it prevents users from:
    /// - Inserting new records
    /// - Updating existing records
    /// - Deleting records
    ///
    /// This role is useful for creating read-only access to the database or
    /// specific tables, ensuring that users cannot modify data even if they
    /// have other roles that would normally allow it.
    /// </summary>
    DenyDataWriter = 8,

    /// <summary>
    /// The deny data reader role.
    /// This is a custom role that explicitly denies data reading permissions
    /// to users. When assigned, it prevents users from:
    /// - Selecting data from tables
    /// - Viewing table contents
    /// - Reading data through views
    ///
    /// This role is useful for restricting access to sensitive data, ensuring
    /// that users cannot read data even if they have other roles that would
    /// normally allow it.
    /// </summary>
    DenyDataReader = 9
}
