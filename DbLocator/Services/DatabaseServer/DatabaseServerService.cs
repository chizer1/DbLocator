using System.ComponentModel.DataAnnotations;
using DbLocator.Db;
using DbLocator.Features.DatabaseServers.CreateDatabaseServer;
using DbLocator.Features.DatabaseServers.DeleteDatabaseServer;
using DbLocator.Features.DatabaseServers.GetDatabaseServerById;
using DbLocator.Features.DatabaseServers.GetDatabaseServers;
using DbLocator.Features.DatabaseServers.UpdateDatabaseServerName;
using DbLocator.Features.DatabaseServers.UpdateDatabaseServerNetwork;
using DbLocator.Features.DatabaseServers.UpdateDatabaseServerStatus;
using DbLocator.Utilities;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Services.DatabaseServer;

/// <summary>
/// Represents network-related information for a database server.
/// </summary>
public record DatabaseServerNetworkInfo
{
    /// <summary>
    /// The host name of the database server.
    /// </summary>
    [StringLength(255)]
    public string HostName { get; init; }

    /// <summary>
    /// The fully qualified domain name of the database server.
    /// </summary>
    [StringLength(255)]
    public string FullyQualifiedDomainName { get; init; }

    /// <summary>
    /// The IP address of the database server.
    /// </summary>
    [StringLength(45)] // IPv6 max length
    [RegularExpression(@"^([0-9]{1,3}\.){3}[0-9]{1,3}$|^([0-9a-fA-F]{1,4}:){7}[0-9a-fA-F]{1,4}$")]
    public string IpAddress { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseServerNetworkInfo"/> class.
    /// </summary>
    /// <param name="hostName">The host name of the database server.</param>
    /// <param name="fullyQualifiedDomainName">The fully qualified domain name of the database server.</param>
    /// <param name="ipAddress">The IP address of the database server.</param>
    public DatabaseServerNetworkInfo(
        string hostName = null,
        string fullyQualifiedDomainName = null,
        string ipAddress = null
    )
    {
        HostName = hostName;
        FullyQualifiedDomainName = fullyQualifiedDomainName;
        IpAddress = ipAddress;
    }
}

/// <summary>
/// Represents a request to create a new database server.
/// </summary>
public record DatabaseServerCreateRequest
{
    /// <summary>
    /// The name of the database server.
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Name { get; init; }

    /// <summary>
    /// Whether the database server is a linked server.
    /// </summary>
    public bool IsLinkedServer { get; init; }

    /// <summary>
    /// Network-related information for the database server.
    /// </summary>
    public DatabaseServerNetworkInfo NetworkInfo { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseServerCreateRequest"/> class.
    /// </summary>
    /// <param name="name">The name of the database server.</param>
    /// <param name="isLinkedServer">Whether the database server is a linked server.</param>
    /// <param name="networkInfo">Network-related information for the database server.</param>
    public DatabaseServerCreateRequest(
        string name,
        bool isLinkedServer,
        DatabaseServerNetworkInfo networkInfo = null
    )
    {
        Name = name;
        IsLinkedServer = isLinkedServer;
        NetworkInfo = networkInfo;
    }
}

/// <summary>
/// Represents a request to update an existing database server.
/// </summary>
public record DatabaseServerUpdateRequest
{
    /// <summary>
    /// The ID of the database server to update.
    /// </summary>
    [Required]
    [Range(1, int.MaxValue)]
    public int DatabaseServerId { get; init; }

    /// <summary>
    /// The new name for the database server.
    /// </summary>
    [StringLength(255)]
    public string Name { get; init; }

    /// <summary>
    /// Network-related information for the database server.
    /// </summary>
    public DatabaseServerNetworkInfo NetworkInfo { get; init; }

    /// <summary>
    /// Whether the database server is a linked server.
    /// </summary>
    public bool? IsLinkedServer { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseServerUpdateRequest"/> class.
    /// </summary>
    /// <param name="databaseServerId">The ID of the database server to update.</param>
    /// <param name="name">The new name for the database server.</param>
    /// <param name="networkInfo">Network-related information for the database server.</param>
    /// <param name="isLinkedServer">Whether the database server is a linked server.</param>
    public DatabaseServerUpdateRequest(
        int databaseServerId,
        string name = null,
        DatabaseServerNetworkInfo networkInfo = null,
        bool? isLinkedServer = null
    )
    {
        DatabaseServerId = databaseServerId;
        Name = name;
        NetworkInfo = networkInfo;
        IsLinkedServer = isLinkedServer;
    }
}

/// <summary>
/// Service for managing database servers.
/// </summary>
internal class DatabaseServerService(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
) : IDatabaseServerService
{
    private readonly CreateDatabaseServerHandler _createDatabaseServer =
        new(dbContextFactory, cache);
    private readonly DeleteDatabaseServerHandler _deleteDatabaseServer =
        new(dbContextFactory, cache);
    private readonly GetDatabaseServersHandler _getDatabaseServers = new(dbContextFactory, cache);
    private readonly GetDatabaseServerByIdHandler _getDatabaseServerById =
        new(dbContextFactory, cache);
    private readonly UpdateDatabaseServerNameHandler _updateDatabaseServerName =
        new(dbContextFactory, cache);
    private readonly UpdateDatabaseServerNetworkHandler _updateDatabaseServerNetwork =
        new(dbContextFactory, cache);
    private readonly UpdateDatabaseServerStatusHandler _updateDatabaseServerStatus =
        new(dbContextFactory, cache);

    /// <summary>
    /// Adds a new database server.
    /// </summary>
    /// <param name="request">The creation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The ID of the created database server.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the request is null.</exception>
    /// <exception cref="ValidationException">Thrown when the request is invalid.</exception>
    private async Task<int> AddDatabaseServer(
        DatabaseServerCreateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var result = await _createDatabaseServer.Handle(
            new CreateDatabaseServerCommand(
                request.Name,
                request.NetworkInfo?.HostName,
                request.NetworkInfo?.FullyQualifiedDomainName,
                request.NetworkInfo?.IpAddress,
                request.IsLinkedServer
            ),
            cancellationToken
        );

        return result;
    }

    /// <summary>
    /// Adds a new database server with basic information.
    /// </summary>
    /// <param name="databaseServerName">The name of the database server.</param>
    /// <param name="isLinkedServer">Whether the database server is a linked server.</param>
    /// <returns>The ID of the created database server.</returns>
    public Task<int> AddDatabaseServer(string databaseServerName, bool isLinkedServer)
    {
        return AddDatabaseServer(
            new DatabaseServerCreateRequest(databaseServerName, isLinkedServer),
            CancellationToken.None
        );
    }

    /// <summary>
    /// Adds a new database server with host name.
    /// </summary>
    /// <param name="databaseServerName">The name of the database server.</param>
    /// <param name="isLinkedServer">Whether the database server is a linked server.</param>
    /// <param name="databaseServerHostName">The host name of the database server.</param>
    /// <returns>The ID of the created database server.</returns>
    public Task<int> AddDatabaseServer(
        string databaseServerName,
        bool isLinkedServer,
        string databaseServerHostName
    )
    {
        return AddDatabaseServer(
            new DatabaseServerCreateRequest(
                databaseServerName,
                isLinkedServer,
                new DatabaseServerNetworkInfo(databaseServerHostName)
            ),
            CancellationToken.None
        );
    }

    /// <summary>
    /// Adds a new database server with host name and IP address.
    /// </summary>
    /// <param name="databaseServerName">The name of the database server.</param>
    /// <param name="isLinkedServer">Whether the database server is a linked server.</param>
    /// <param name="databaseServerHostName">The host name of the database server.</param>
    /// <param name="databaseServerIpAddress">The IP address of the database server.</param>
    /// <returns>The ID of the created database server.</returns>
    public Task<int> AddDatabaseServer(
        string databaseServerName,
        bool isLinkedServer,
        string databaseServerHostName,
        string databaseServerIpAddress
    )
    {
        return AddDatabaseServer(
            new DatabaseServerCreateRequest(
                databaseServerName,
                isLinkedServer,
                new DatabaseServerNetworkInfo(databaseServerHostName, null, databaseServerIpAddress)
            ),
            CancellationToken.None
        );
    }

    /// <summary>
    /// Adds a new database server with complete network information.
    /// </summary>
    /// <param name="databaseServerName">The name of the database server.</param>
    /// <param name="isLinkedServer">Whether the database server is a linked server.</param>
    /// <param name="databaseServerHostName">The host name of the database server.</param>
    /// <param name="databaseServerIpAddress">The IP address of the database server.</param>
    /// <param name="databaseServerFullyQualifiedDomainName">The fully qualified domain name of the database server.</param>
    /// <returns>The ID of the created database server.</returns>
    public Task<int> AddDatabaseServer(
        string databaseServerName,
        bool isLinkedServer,
        string databaseServerHostName,
        string databaseServerIpAddress,
        string databaseServerFullyQualifiedDomainName
    )
    {
        return AddDatabaseServer(
            new DatabaseServerCreateRequest(
                databaseServerName,
                isLinkedServer,
                new DatabaseServerNetworkInfo(
                    databaseServerHostName,
                    databaseServerFullyQualifiedDomainName,
                    databaseServerIpAddress
                )
            ),
            CancellationToken.None
        );
    }

    /// <summary>
    /// Deletes a database server.
    /// </summary>
    /// <param name="databaseServerId">The ID of the database server to delete.</param>
    /// <exception cref="ArgumentException">Thrown when the database server ID is invalid.</exception>
    public async Task DeleteDatabaseServer(int databaseServerId)
    {
        if (databaseServerId <= 0)
        {
            throw new ArgumentException(
                "Database server ID must be greater than zero",
                nameof(databaseServerId)
            );
        }

        await _deleteDatabaseServer.Handle(
            new DeleteDatabaseServerCommand(databaseServerId),
            CancellationToken.None
        );
    }

    /// <summary>
    /// Gets all database servers.
    /// </summary>
    /// <returns>A list of database servers.</returns>
    public async Task<List<Domain.DatabaseServer>> GetDatabaseServers()
    {
        var result = await _getDatabaseServers.Handle(
            new GetDatabaseServersQuery(),
            CancellationToken.None
        );
        return result.ToList();
    }

    /// <summary>
    /// Gets a database server by ID.
    /// </summary>
    /// <param name="databaseServerId">The ID of the database server.</param>
    /// <returns>The database server.</returns>
    /// <exception cref="ArgumentException">Thrown when the database server ID is invalid.</exception>
    public async Task<Domain.DatabaseServer> GetDatabaseServer(int databaseServerId)
    {
        if (databaseServerId <= 0)
        {
            throw new ArgumentException(
                "Database server ID must be greater than zero",
                nameof(databaseServerId)
            );
        }

        return await _getDatabaseServerById.Handle(
            new GetDatabaseServerByIdQuery(databaseServerId),
            CancellationToken.None
        );
    }

    /// <summary>
    /// Updates a database server.
    /// </summary>
    /// <param name="request">The update request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ArgumentNullException">Thrown when the request is null.</exception>
    /// <exception cref="ValidationException">Thrown when the request is invalid.</exception>
    private async Task UpdateDatabaseServer(
        DatabaseServerUpdateRequest request,
        CancellationToken cancellationToken = default
    )
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Name != null)
        {
            await UpdateDatabaseServerName(
                request.DatabaseServerId,
                request.Name,
                cancellationToken
            );
        }

        if (request.NetworkInfo != null)
        {
            await UpdateDatabaseServerNetwork(
                request.DatabaseServerId,
                request.NetworkInfo.HostName,
                request.NetworkInfo.FullyQualifiedDomainName,
                request.NetworkInfo.IpAddress,
                cancellationToken
            );
        }

        if (request.IsLinkedServer.HasValue)
        {
            await UpdateDatabaseServerStatus(
                request.DatabaseServerId,
                request.IsLinkedServer.Value,
                cancellationToken
            );
        }
    }

    /// <summary>
    /// Updates a database server with complete information.
    /// </summary>
    /// <param name="databaseServerId">The ID of the database server.</param>
    /// <param name="databaseServerName">The new name of the database server.</param>
    /// <param name="databaseServerHostName">The host name of the database server.</param>
    /// <param name="databaseServerFullyQualifiedDomainName">The fully qualified domain name of the database server.</param>
    /// <param name="databaseServerIpAddress">The IP address of the database server.</param>
    /// <param name="isLinkedServer">Whether the database server is a linked server.</param>
    public Task UpdateDatabaseServer(
        int databaseServerId,
        string databaseServerName,
        string databaseServerHostName,
        string databaseServerFullyQualifiedDomainName,
        string databaseServerIpAddress,
        bool isLinkedServer
    )
    {
        return UpdateDatabaseServer(
            new DatabaseServerUpdateRequest(
                databaseServerId,
                databaseServerName,
                new DatabaseServerNetworkInfo(
                    databaseServerHostName,
                    databaseServerFullyQualifiedDomainName,
                    databaseServerIpAddress
                ),
                isLinkedServer
            ),
            CancellationToken.None
        );
    }

    /// <summary>
    /// Updates a database server's name.
    /// </summary>
    /// <param name="databaseServerId">The ID of the database server.</param>
    /// <param name="databaseServerName">The new name of the database server.</param>
    public Task UpdateDatabaseServer(int databaseServerId, string databaseServerName)
    {
        return UpdateDatabaseServer(
            new DatabaseServerUpdateRequest(databaseServerId, databaseServerName),
            CancellationToken.None
        );
    }

    /// <summary>
    /// Updates a database server's network information.
    /// </summary>
    /// <param name="databaseServerId">The ID of the database server.</param>
    /// <param name="databaseServerHostName">The host name of the database server.</param>
    /// <param name="databaseServerFullyQualifiedDomainName">The fully qualified domain name of the database server.</param>
    /// <param name="databaseServerIpAddress">The IP address of the database server.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="ArgumentException">Thrown when no network identifier is provided.</exception>
    private async Task UpdateDatabaseServerNetwork(
        int databaseServerId,
        string databaseServerHostName,
        string databaseServerFullyQualifiedDomainName,
        string databaseServerIpAddress,
        CancellationToken cancellationToken = default
    )
    {
        if (
            string.IsNullOrWhiteSpace(databaseServerHostName)
            && string.IsNullOrWhiteSpace(databaseServerFullyQualifiedDomainName)
            && string.IsNullOrWhiteSpace(databaseServerIpAddress)
        )
        {
            throw new ArgumentException(
                "At least one network identifier (host name, FQDN, or IP address) is required",
                nameof(databaseServerHostName)
            );
        }

        var server = await GetDatabaseServer(databaseServerId);
        if (
            databaseServerHostName != server.HostName
            || databaseServerFullyQualifiedDomainName != server.FullyQualifiedDomainName
            || databaseServerIpAddress != server.IpAddress
        )
        {
            await using var dbContext = dbContextFactory.CreateDbContext();
            var databaseServer =
                await dbContext
                    .Set<DatabaseServerEntity>()
                    .FirstOrDefaultAsync(
                        ds => ds.DatabaseServerId == databaseServerId,
                        cancellationToken
                    )
                ?? throw new KeyNotFoundException(
                    $"Database server with ID {databaseServerId} not found"
                );

            if (!string.IsNullOrEmpty(databaseServerHostName))
                databaseServer.DatabaseServerHostName = databaseServerHostName;

            if (!string.IsNullOrEmpty(databaseServerFullyQualifiedDomainName))
                databaseServer.DatabaseServerFullyQualifiedDomainName =
                    databaseServerFullyQualifiedDomainName;

            if (!string.IsNullOrEmpty(databaseServerIpAddress))
                databaseServer.DatabaseServerIpaddress = databaseServerIpAddress;

            dbContext.Set<DatabaseServerEntity>().Update(databaseServer);
            await dbContext.SaveChangesAsync(cancellationToken);

            if (cache != null)
            {
                await cache.Remove("databaseServers");
                await cache.Remove($"databaseServer-id-{databaseServerId}");
            }
        }
    }

    /// <summary>
    /// Updates a database server's network information.
    /// </summary>
    /// <param name="databaseServerId">The ID of the database server.</param>
    /// <param name="databaseServerFullyQualifiedDomainName">The fully qualified domain name of the database server.</param>
    /// <param name="databaseServerIpAddress">The IP address of the database server.</param>
    public Task UpdateDatabaseServer(
        int databaseServerId,
        string databaseServerFullyQualifiedDomainName,
        string databaseServerIpAddress
    )
    {
        return UpdateDatabaseServer(
            new DatabaseServerUpdateRequest(
                databaseServerId,
                null,
                new DatabaseServerNetworkInfo(
                    null,
                    databaseServerFullyQualifiedDomainName,
                    databaseServerIpAddress
                )
            ),
            CancellationToken.None
        );
    }

    /// <summary>
    /// Updates a database server's linked server status.
    /// </summary>
    /// <param name="databaseServerId">The ID of the database server.</param>
    /// <param name="isLinkedServer">Whether the database server is a linked server.</param>
    public Task UpdateDatabaseServer(int databaseServerId, bool isLinkedServer)
    {
        return UpdateDatabaseServer(
            new DatabaseServerUpdateRequest(databaseServerId, null, null, isLinkedServer),
            CancellationToken.None
        );
    }

    /// <summary>
    /// Updates a database server's name.
    /// </summary>
    /// <param name="databaseServerId">The ID of the database server.</param>
    /// <param name="databaseServerName">The new name of the database server.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task UpdateDatabaseServerName(
        int databaseServerId,
        string databaseServerName,
        CancellationToken cancellationToken = default
    )
    {
        var server = await GetDatabaseServer(databaseServerId);
        if (databaseServerName != server.Name)
        {
            await _updateDatabaseServerName.Handle(
                new UpdateDatabaseServerNameCommand(databaseServerId, databaseServerName),
                cancellationToken
            );
        }
    }

    /// <summary>
    /// Updates a database server's linked server status.
    /// </summary>
    /// <param name="databaseServerId">The ID of the database server.</param>
    /// <param name="isLinkedServer">Whether the database server is a linked server.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task UpdateDatabaseServerStatus(
        int databaseServerId,
        bool isLinkedServer,
        CancellationToken cancellationToken = default
    )
    {
        var server = await GetDatabaseServer(databaseServerId);
        if (isLinkedServer != server.IsLinkedServer)
        {
            await _updateDatabaseServerStatus.Handle(
                new UpdateDatabaseServerStatusCommand(databaseServerId, isLinkedServer),
                cancellationToken
            );
        }
    }
}
