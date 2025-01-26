namespace DbLocatorTests.Fixtures;

public class DbLocatorFixture : IDisposable
{
    public DbLocator.DbLocator DbLocator { get; private set; }
    private const string connString =
        "Server=localhost;Database=DbLocator;User Id=sa;Password=1StrongPwd!!;Encrypt=True;TrustServerCertificate=True;";

    public DbLocatorFixture()
    {
        DbLocator = new DbLocator.DbLocator(connString);
    }

    public void Dispose() { }
}

[CollectionDefinition("DbLocator")]
public class DbLocatorCollection : ICollectionFixture<DbLocatorFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
