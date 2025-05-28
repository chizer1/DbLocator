#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUsers.GetDatabaseUsers;

internal record GetDatabaseUsersQuery;

internal sealed class GetDatabaseUsersQueryValidator : AbstractValidator<GetDatabaseUsersQuery>
{
    internal GetDatabaseUsersQueryValidator() { }
}

internal class GetDatabaseUsersHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<List<DatabaseUser>> Handle(
        GetDatabaseUsersQuery request,
        CancellationToken cancellationToken = default
    )
    {
        await new GetDatabaseUsersQueryValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        var cacheKey = "databaseUsers";
        if (_cache != null)
        {
            var cachedData = await _cache.GetCachedData<List<DatabaseUser>>(cacheKey);
            if (cachedData != null)
            {
                return cachedData;
            }
        }

        var databaseUsers = await GetDatabaseUsersFromDatabase(cancellationToken);

        if (_cache != null)
        {
            await _cache.CacheData(cacheKey, databaseUsers);
        }

        return databaseUsers;
    }

    private async Task<List<DatabaseUser>> GetDatabaseUsersFromDatabase(
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        var databaseUserEntities = await dbContext
            .Set<DatabaseUserEntity>()
            .AsNoTracking()
            .Include(u => u.Databases)
            .ThenInclude(ud => ud.Database)
            .ThenInclude(d => d.DatabaseServer)
            .Include(u => u.Databases)
            .ThenInclude(ud => ud.Database)
            .ThenInclude(d => d.DatabaseType)
            .Include(u => u.UserRoles)
            .ToListAsync(cancellationToken);

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

#nullable disable
