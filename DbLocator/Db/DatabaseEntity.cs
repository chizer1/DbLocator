namespace DbLocator.Db;

internal class DatabaseEntity
{
    internal int DatabaseId { get; set; }

    internal string DatabaseName { get; set; }

    internal int DatabaseServerId { get; set; }

    internal byte DatabaseTypeId { get; set; }

    internal byte DatabaseStatusId { get; set; }

    internal bool UseTrustedConnection { get; set; }

    internal virtual ICollection<ConnectionEntity> Connections { get; set; } = [];

    internal virtual ICollection<DatabaseUserEntity> DatabaseUsers { get; set; } = [];

    internal virtual DatabaseServerEntity DatabaseServer { get; set; }

    internal virtual DatabaseTypeEntity DatabaseType { get; set; }
}
