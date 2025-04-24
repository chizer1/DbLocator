using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUsers;

internal class GetDatabaseUserQuery
{
    public int DatabaseUserId { get; set; }
}

internal sealed class GetDatabaseUserQueryValidator : AbstractValidator<GetDatabaseUserQuery>
{
    internal GetDatabaseUserQueryValidator()
    {
        RuleFor(x => x.DatabaseUserId).GreaterThan(0);
    }
}

internal class GetDatabaseUser(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    internal async Task<DatabaseUser> Handle(GetDatabaseUserQuery query)
    {
        await new GetDatabaseUserQueryValidator().ValidateAndThrowAsync(query);

        var cacheKey = $"databaseUser-{query.DatabaseUserId}";
        var cachedData = await cache?.GetCachedData<DatabaseUser>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        var databaseUser = await GetDatabaseUserFromDatabase(
            dbContextFactory,
            query.DatabaseUserId
        );
        await cache?.CacheData(cacheKey, databaseUser);

        return databaseUser;
    }

    private static async Task<DatabaseUser> GetDatabaseUserFromDatabase(
        IDbContextFactory<DbLocatorContext> dbContextFactory,
        int databaseUserId
    )
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseUserEntity = await dbContext
            .Set<DatabaseUserEntity>()
            .AsNoTracking()
            .Include(u => u.Databases)
            .ThenInclude(ud => ud.Database)
            .ThenInclude(d => d.DatabaseServer)
            .Include(u => u.Databases)
            .ThenInclude(ud => ud.Database)
            .ThenInclude(d => d.DatabaseType)
            .Include(u => u.UserRoles)
            .FirstOrDefaultAsync(u => u.DatabaseUserId == databaseUserId);

        if (databaseUserEntity == null)
        {
            throw new KeyNotFoundException($"Database user with ID {databaseUserId} not found.");
        }

        return new DatabaseUser(
            databaseUserEntity.DatabaseUserId,
            databaseUserEntity.UserName,
            [
                .. databaseUserEntity
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
                                ud.Database.DatabaseServer.DatabaseServerFullyQualifiedDomainName,
                                ud.Database.DatabaseServer.IsLinkedServer
                            )
                            : null!,
                        (Status)ud.Database.DatabaseStatusId,
                        ud.Database.UseTrustedConnection
                    ))
            ],
            [.. databaseUserEntity.UserRoles.Select(ur => (DatabaseRole)ur.DatabaseRoleId)]
        );
    }
}
