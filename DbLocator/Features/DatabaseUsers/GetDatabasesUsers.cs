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
            .Include(u => u.Databases)
            .ThenInclude(ud => ud.Database)
            .ThenInclude(d => d.DatabaseServer)
            .Include(u => u.Databases)
            .ThenInclude(ud => ud.Database)
            .ThenInclude(d => d.DatabaseType)
            .Include(u => u.UserRoles)
            .ToListAsync();

        var databaseUsers = databaseUserEntities
            .Select(user => new DatabaseUser(
                user.DatabaseUserId,
                user.UserName,
                [
                    .. user
                        .Databases.Where(ud => ud.Database != null)
                        .Select(ud => new Database(
                            ud.Database.DatabaseId,
                            ud.Database.DatabaseName,
                            ud.Database.DatabaseType != null
                                ? new DatabaseType(
                                    ud.Database.DatabaseType.DatabaseTypeId,
                                    ud.Database.DatabaseType.DatabaseTypeName
                                )
                                : null!,
                            ud.Database.DatabaseServer != null
                                ? new DatabaseServer(
                                    ud.Database.DatabaseServer.DatabaseServerId,
                                    ud.Database.DatabaseServer.DatabaseServerName,
                                    ud.Database.DatabaseServer.DatabaseServerIpaddress,
                                    ud.Database.DatabaseServer.DatabaseServerHostName,
                                    ud.Database
                                        .DatabaseServer
                                        .DatabaseServerFullyQualifiedDomainName,
                                    ud.Database.DatabaseServer.IsLinkedServer
                                )
                                : null!,
                            (Status)ud.Database.DatabaseStatusId,
                            ud.Database.UseTrustedConnection
                        ))
                ],
                [.. user.UserRoles.Select(ur => (DatabaseRole)ur.DatabaseRoleId)]
            ))
            .ToList();

        return databaseUsers;
    }
}
