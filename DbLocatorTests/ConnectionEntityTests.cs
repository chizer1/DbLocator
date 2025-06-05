using DbLocator.Db;
using Xunit;

namespace DbLocatorTests;

public class ConnectionEntityTests
{
    [Fact]
    public void Tenant_Property_CanBeSetAndRetrieved()
    {
        // Arrange
        var connection = new ConnectionEntity();
        var tenant = new TenantEntity
        {
            TenantId = 1,
            TenantName = "Test Tenant",
            TenantCode = "TEST",
            TenantStatusId = 1
        };

        // Act
        connection.Tenant = tenant;

        // Assert
        Assert.NotNull(connection.Tenant);
        Assert.Equal(tenant.TenantId, connection.Tenant.TenantId);
        Assert.Equal(tenant.TenantName, connection.Tenant.TenantName);
        Assert.Equal(tenant.TenantCode, connection.Tenant.TenantCode);
        Assert.Equal(tenant.TenantStatusId, connection.Tenant.TenantStatusId);
    }

    [Fact]
    public void Tenant_Property_CanBeSetToNull()
    {
        // Arrange
        var connection = new ConnectionEntity();
        var tenant = new TenantEntity
        {
            TenantId = 1,
            TenantName = "Test Tenant",
            TenantCode = "TEST",
            TenantStatusId = 1
        };
        connection.Tenant = tenant;

        // Act
        connection.Tenant = null;

        // Assert
        Assert.Null(connection.Tenant);
    }
} 