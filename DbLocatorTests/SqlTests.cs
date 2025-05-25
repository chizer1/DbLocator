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
        // Arrange
        string validIdentifier = "Valid_Identifier123";

        // Act
        string result = Sql.SanitizeSqlIdentifier(validIdentifier);

        // Assert
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
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Sql.SanitizeSqlIdentifier(invalidIdentifier));
    }

    [Fact]
    public void SanitizeSqlIdentifier_ShouldThrowArgumentNullException_WhenInputIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => Sql.SanitizeSqlIdentifier(null));
    }

    [Fact]
    public async Task ExecuteSqlCommandAsync_ShouldExecuteCommand_WhenValidInput()
    {
        // Arrange
        string commandText = "SELECT 1";

        // Act
        await Sql.ExecuteSqlCommandAsync(_dbLocatorContext, commandText);

        // Assert
        // No exception means the command executed successfully
    }

    [Fact]
    public async Task ExecuteSqlCommandAsync_ShouldThrowArgumentException_WhenLinkedServerHostNameIsNull()
    {
        // Arrange
        string commandText = "SELECT 1";

        // Act & Assert
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
        // Arrange
        string commandText = "SELECT * FROM Users";
        string linkedServerHostName = "sqlserver_server_2";

        // Create the Users table for the test
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

        // Act
        await Sql.ExecuteSqlCommandAsync(
            _dbLocatorContext,
            commandText,
            isLinkedServer: true,
            linkedServerHostName: linkedServerHostName
        );

        // Assert
        // No exception means the command executed successfully
    }
}
