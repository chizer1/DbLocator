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
            .Include(c => c.Databases)
            .ThenInclude(d => d.DatabaseServer)
            .Include(d => d.UserRoles)
            .ToListAsync();

        var databaseUsers = databaseUserEntities
            .Select(d => new DatabaseUser(
                d.DatabaseUserId,
                d.UserName,
                [
                    .. d.Databases.Select(db => new Database(
                        db.DatabaseId,
                        db.DatabaseName,
                        db.DatabaseType != null
                            ? new DatabaseType(
                                db.DatabaseType.DatabaseTypeId,
                                db.DatabaseType.DatabaseTypeName
                            )
                            : null!,
                        db.DatabaseServer != null
                            ? new DatabaseServer(
                                db.DatabaseServer.DatabaseServerId,
                                db.DatabaseServer.DatabaseServerName,
                                db.DatabaseServer.DatabaseServerIpaddress,
                                db.DatabaseServer.DatabaseServerHostName,
                                db.DatabaseServer.DatabaseServerFullyQualifiedDomainName,
                                db.DatabaseServer.IsLinkedServer
                            )
                            : null!,
                        (Status)db.DatabaseStatusId,
                        db.UseTrustedConnection
                    ))
                ],
                [.. d.UserRoles.Select(ur => (DatabaseRole)ur.DatabaseRoleId)]
            ))
            .ToList();

        return databaseUsers;
    }
}
