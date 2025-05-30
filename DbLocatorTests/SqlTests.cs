using DbLocator;
using DbLocator.Db;
using DbLocatorTests.Fixtures;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class SqlTests(DbLocatorFixture dbLocatorFixture)
{
    private readonly DbLocatorContext _dbLocatorContext = DbContextFactory
        .CreateDbContextFactory(dbLocatorFixture.ConnectionString)
        .CreateDbContext();

    [Fact]
    public void SanitizeSqlIdentifier_ShouldReturnInput_WhenValidIdentifier()
    {
        string validIdentifier = "Valid_Identifier123";

        string result = Sql.SanitizeSqlIdentifier(validIdentifier);

        Assert.Equal(validIdentifier, result);
    }

    [Theory]
    [InlineData("Invalid-Identifier")]
    [InlineData("Invalid Identifier")]
    [InlineData("Invalid!Identifier")]
    [InlineData("")]
    public void SanitizeSqlIdentifier_ShouldThrowArgumentException_WhenInvalidIdentifier(
        string invalidIdentifier
    )
    {
        Assert.Throws<ArgumentException>(() => Sql.SanitizeSqlIdentifier(invalidIdentifier));
    }

    [Fact]
    public void SanitizeSqlIdentifier_ShouldThrowArgumentNullException_WhenInputIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => Sql.SanitizeSqlIdentifier(null));
    }

    [Fact]
    public async Task ExecuteSqlCommandAsync_ShouldExecuteCommand_WhenValidInput()
    {
        string commandText = "SELECT 1";

        await Sql.ExecuteSqlCommandAsync(_dbLocatorContext, commandText);
    }

    [Fact]
    public async Task ExecuteSqlCommandAsync_ShouldThrowArgumentException_WhenLinkedServerHostNameIsNull()
    {
        string commandText = "SELECT 1";

        await Assert.ThrowsAsync<ArgumentException>(
            () =>
                Sql.ExecuteSqlCommandAsync(
                    _dbLocatorContext,
                    commandText,
                    isLinkedServer: true,
                    linkedServerHostName: null
                )
        );
    }

    [Fact]
    public async Task ExecuteSqlCommandAsync_ShouldEscapeCommandText_WhenLinkedServer()
    {
        string commandText = "SELECT * FROM Users";
        string linkedServerHostName = "sqlserver_server_2";

        string createTableCommand =
            @"
            IF OBJECT_ID('Users', 'U') IS NULL
            CREATE TABLE Users (
                Id INT PRIMARY KEY IDENTITY(1,1),
                Name NVARCHAR(100) NOT NULL
            );";

        await Sql.ExecuteSqlCommandAsync(
            _dbLocatorContext,
            createTableCommand,
            isLinkedServer: true,
            linkedServerHostName: linkedServerHostName
        );

        await Sql.ExecuteSqlCommandAsync(
            _dbLocatorContext,
            commandText,
            isLinkedServer: true,
            linkedServerHostName: linkedServerHostName
        );
    }
}
