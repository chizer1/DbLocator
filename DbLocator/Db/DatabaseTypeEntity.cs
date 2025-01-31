namespace DbLocator.Db;

internal class DatabaseTypeEntity
{
    internal byte DatabaseTypeId { get; set; }

    internal string DatabaseTypeName { get; set; }

    internal virtual ICollection<DatabaseEntity> Databases { get; set; } = [];
}
