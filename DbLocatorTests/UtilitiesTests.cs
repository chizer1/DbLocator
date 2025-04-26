using DbLocatorTests.Fixtures;
using Microsoft.Data.SqlClient;

namespace DbLocatorTests
{
    [Collection("DbLocator")]
    public class UtilitiesTests(DbLocatorFixture dbLocatorFixture)
    {
        private readonly string _dbLocatorConnectionString = dbLocatorFixture
            .DbLocator
            .ConnectionString;

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
        [InlineData(null)]
        public void SanitizeSqlIdentifier_ShouldThrowArgumentException_WhenInvalidIdentifier(
            string invalidIdentifier
        )
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => Sql.SanitizeSqlIdentifier(invalidIdentifier));
        }

        [Fact]
        public async Task ExecuteSqlCommandAsync_ShouldExecuteCommand_WhenValidInput()
        {
            // Arrange
            string commandText = "SELECT 1";

            // Act
            await ExecuteSqlCommandAsync(commandText);

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
                    ExecuteSqlCommandAsync(
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
            string linkedServerHostName = "localhost";
            // Create the Users table for the test
            string createTableCommand =
                @"
                IF OBJECT_ID('Users', 'U') IS NULL
                CREATE TABLE Users (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    Name NVARCHAR(100) NOT NULL
                );";

            await ExecuteSqlCommandAsync(createTableCommand);

            // Act
            await ExecuteSqlCommandAsync(
                commandText,
                isLinkedServer: true,
                linkedServerHostName: linkedServerHostName
            );

            // Assert
            // No exception means the command executed successfully
        }

        private async Task ExecuteSqlCommandAsync(
            string commandText,
            bool isLinkedServer = false,
            string linkedServerHostName = null
        )
        {
            if (isLinkedServer && string.IsNullOrEmpty(linkedServerHostName))
                throw new ArgumentException("Linked server host name cannot be null or empty.");

            if (isLinkedServer)
                commandText =
                    $"exec('{commandText.Replace("'", "''")}') at [{linkedServerHostName}];";

            using var connection = new SqlConnection(_dbLocatorConnectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(commandText, connection);
            await command.ExecuteNonQueryAsync();
        }
    }
}
