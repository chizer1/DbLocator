#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUsers.GetDatabaseUser;

internal record GetDatabaseUserQuery(int DatabaseUserId);

internal sealed class GetDatabaseUserQueryValidator : AbstractValidator<GetDatabaseUserQuery>
{
    internal GetDatabaseUserQueryValidator()
    {
        RuleFor(x => x.DatabaseUserId)
            .GreaterThan(0)
            .WithMessage("Database User Id must be greater than 0.");
    }
}

internal class GetDatabaseUserHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<DatabaseUser> Handle(
        GetDatabaseUserQuery request,
        CancellationToken cancellationToken = default
    )
    {
        await new GetDatabaseUserQueryValidator().ValidateAndThrowAsync(request, cancellationToken);

        var cacheKey = $"databaseUser-{request.DatabaseUserId}";
        if (_cache != null)
        {
            var cachedData = await _cache.GetCachedData<DatabaseUser>(cacheKey);
            if (cachedData != null)
                return cachedData;
        }

        var databaseUser = await GetDatabaseUserFromDatabase(
            request.DatabaseUserId,
            cancellationToken
        );

        if (_cache != null)
            await _cache.CacheData(cacheKey, databaseUser);
        return databaseUser;
    }

    private async Task<DatabaseUser> GetDatabaseUserFromDatabase(
        int databaseUserId,
        CancellationToken cancellationToken
    )
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        var databaseUserEntity =
            await dbContext
                .Set<DatabaseUserEntity>()
                .AsNoTracking()
                .Include(u => u.Databases)
                .ThenInclude(ud => ud.Database)
                .ThenInclude(d => d.DatabaseServer)
                .Include(u => u.Databases)
                .ThenInclude(ud => ud.Database)
                .ThenInclude(d => d.DatabaseType)
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.DatabaseUserId == databaseUserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Database user with ID {databaseUserId} not found.");

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
