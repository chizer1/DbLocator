using System.Runtime.InteropServices;
using DbLocator;
using DbLocator.Domain;
using DbLocatorTests.Fixtures;
using Microsoft.Data.SqlClient;

namespace DbLocatorTests;

[Collection("DbLocator")]
public class ConnectionTests(DbLocatorFixture dbLocatorFixture)
{
    private readonly Locator _dbLocator = dbLocatorFixture.DbLocator;
    private readonly int _databaseServerId = dbLocatorFixture.LocalhostServerId;

    [Fact]
    public async Task AddConnection()
    {
        var connectionId = await GetConnectionId();

        var connections = await _dbLocator.GetConnections();
        Assert.Contains(connections, cn => cn.Id == connectionId);
    }

    [Fact]
    public async Task UseConnection()
    {
        var connectionId = await GetConnectionId();

        var connection_create = await _dbLocator.GetConnection(
            connectionId,
            [DatabaseRole.DdlAdmin]
        );

        using (var sqlConnection = connection_create)
        {
            await sqlConnection.OpenAsync();

            using var command = sqlConnection.CreateCommand();
            command.CommandText = "create table test_table (id int primary key)";
            command.CommandType = System.Data.CommandType.Text;
            var exception = await Record.ExceptionAsync(command.ExecuteNonQueryAsync);
            Assert.Null(exception);

            await sqlConnection.CloseAsync();
        }

        var connection_write = await _dbLocator.GetConnection(
            connectionId,
            [DatabaseRole.DataWriter]
        );

        using (var sqlConnection = connection_write)
        {
            await sqlConnection.OpenAsync();
            using var command = sqlConnection.CreateCommand();
            command.CommandText = "insert into test_table (id) values (0)";
            command.CommandType = System.Data.CommandType.Text;
            var exception = await Record.ExceptionAsync(command.ExecuteNonQueryAsync);
            Assert.Null(exception);
            await sqlConnection.CloseAsync();
        }

        var connection_read = await _dbLocator.GetConnection(
            connectionId,
            [DatabaseRole.DataReader]
        );

        using (var sqlConnection = connection_read)
        {
            await sqlConnection.OpenAsync();
            using var command = sqlConnection.CreateCommand();
            command.CommandText = "select * from test_table";
            command.CommandType = System.Data.CommandType.Text;

            var reader = await command.ExecuteReaderAsync();
            Assert.True(reader.HasRows);
            while (await reader.ReadAsync())
            {
                Assert.Equal(0, reader.GetInt32(0));
            }
        }

        var connection_denyread = await _dbLocator.GetConnection(
            connectionId,
            [DatabaseRole.DenyDataReader]
        );

        using (var sqlConnection = connection_denyread)
        {
            await sqlConnection.OpenAsync();
            using var command = sqlConnection.CreateCommand();
            command.CommandText = "select * from test_table";
            command.CommandType = System.Data.CommandType.Text;

            await Assert.ThrowsAsync<SqlException>(command.ExecuteReaderAsync);
        }
    }

    [Fact]
    public async Task ReuseConnectionUser()
    {
        var connectionId = await GetConnectionId();

        using (
            var sqlConnection = await _dbLocator.GetConnection(
                connectionId,
                [DatabaseRole.DataReader]
            )
        )
        {
            await sqlConnection.OpenAsync();
            using var command = sqlConnection.CreateCommand();
            command.CommandText = "select 1";
            command.CommandType = System.Data.CommandType.Text;

            var reader = await command.ExecuteReaderAsync();
            Assert.True(reader.HasRows);
        }

        using (
            var sqlConnection = await _dbLocator.GetConnection(
                connectionId,
                [DatabaseRole.DataReader]
            )
        )
        {
            await sqlConnection.OpenAsync();
            using var command = sqlConnection.CreateCommand();
            command.CommandText = "select 1";
            command.CommandType = System.Data.CommandType.Text;

            var reader = await command.ExecuteReaderAsync();
            Assert.True(reader.HasRows);
        }
    }

    private async Task<int> GetConnectionId()
    {
        var tenantName = TestHelpers.GetRandomString();
        var tenantId = await _dbLocator.AddTenant(tenantName);

        var databaseTypeName = TestHelpers.GetRandomString();
        var databaseTypeId = await _dbLocator.AddDatabaseType(databaseTypeName);

        var databaseName = TestHelpers.GetRandomString();
        var databaseId = await _dbLocator.AddDatabase(
            databaseName,
            _databaseServerId,
            databaseTypeId,
            Status.Active
        );

        return await _dbLocator.AddConnection(tenantId, databaseId);
    }
}
