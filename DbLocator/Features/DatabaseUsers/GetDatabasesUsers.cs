using System.Text.Json;
using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUsers;

internal class GetDatabaseUsersQuery { }

internal sealed class GetDatabaseUsersQueryValidator : AbstractValidator<GetDatabaseUsersQuery>
{
    internal GetDatabaseUsersQueryValidator() { }
}

internal class GetDatabaseUsers(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    public async Task<List<DatabaseUser>> Handle(GetDatabaseUsersQuery query)
    {
        await new GetDatabaseUsersQueryValidator().ValidateAndThrowAsync(query);

        var cacheKey = "databaseUsers";

        var cachedData = await cache?.GetCachedData<List<DatabaseUser>>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        var databaseUsers = await GetDatabaseUsersFromDatabase(dbContextFactory);
        await cache?.CacheData(cacheKey, databaseUsers);

        return databaseUsers;
    }

    private static async Task<List<DatabaseUser>> GetDatabaseUsersFromDatabase(
        IDbContextFactory<DbLocatorContext> dbContextFactory
    )
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseUserEntities = await dbContext
            .Set<DatabaseUserEntity>()
            .Include(c => c.Database)
            .ThenInclude(d => d.DatabaseServer)
            .Include(c => c.Database)
            .ThenInclude(d => d.DatabaseType)
            .Include(d => d.UserRoles)
            .ToListAsync();

        var databaseUsers = databaseUserEntities
            .Select(d => new DatabaseUser(
                d.DatabaseUserId,
                d.UserName,
                d.Database != null
                    ? new Database(
                        d.Database.DatabaseId,
                        d.Database.DatabaseName,
                        d.Database.DatabaseType != null
                            ? new DatabaseType(
                                d.Database.DatabaseType.DatabaseTypeId,
                                d.Database.DatabaseType.DatabaseTypeName
                            )
                            : null!,
                        d.Database.DatabaseServer != null
                            ? new DatabaseServer(
                                d.Database.DatabaseServer.DatabaseServerId,
                                d.Database.DatabaseServer.DatabaseServerName,
                                d.Database.DatabaseServer.DatabaseServerIpaddress,
                                d.Database.DatabaseServer.DatabaseServerHostName,
                                d.Database.DatabaseServer.DatabaseServerFullyQualifiedDomainName,
                                d.Database.DatabaseServer.IsLinkedServer
                            )
                            : null!,
                        (Status)d.Database.DatabaseStatusId,
                        d.Database.UseTrustedConnection
                    )
                    : null!,
                [.. d.UserRoles.Select(ur => (DatabaseRole)ur.DatabaseRoleId)]
            ))
            .ToList();

        return databaseUsers;
    }
}
