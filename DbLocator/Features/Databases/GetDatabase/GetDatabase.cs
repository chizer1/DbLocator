#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Databases.GetDatabase;

internal record GetDatabaseQuery(int Id);

internal sealed class GetDatabaseQueryValidator : AbstractValidator<GetDatabaseQuery>
{
    internal GetDatabaseQueryValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Database Id must be greater than 0.");
    }
}

internal class GetDatabaseHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<Database> Handle(
        GetDatabaseQuery request,
        CancellationToken cancellationToken = default
    )
    {
        await new GetDatabaseQueryValidator().ValidateAndThrowAsync(request, cancellationToken);

        const string cacheKeyPrefix = "database-id-";
        var cacheKey = $"{cacheKeyPrefix}{request.Id}";

        if (_cache != null)
        {
            var cachedData = await _cache.GetCachedData<Database>(cacheKey);
            if (cachedData != null)
                return cachedData;
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var database =
            await dbContext
                .Set<DatabaseEntity>()
                .Include(d => d.DatabaseType)
                .Include(d => d.DatabaseServer)
                .FirstOrDefaultAsync(d => d.DatabaseId == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Database with ID {request.Id} not found.");

        var result = new Database(
            database.DatabaseId,
            database.DatabaseName,
            new DatabaseType(database.DatabaseTypeId, database.DatabaseType.DatabaseTypeName),
            new DatabaseServer(
                database.DatabaseServerId,
                database.DatabaseServer.DatabaseServerName,
                database.DatabaseServer.DatabaseServerHostName,
                database.DatabaseServer.DatabaseServerIpaddress,
                database.DatabaseServer.DatabaseServerFullyQualifiedDomainName,
                database.DatabaseServer.IsLinkedServer
            ),
            (Status)database.DatabaseStatusId,
            database.UseTrustedConnection
        );

        if (_cache != null)
            await _cache.CacheData(cacheKey, result);

        return result;
    }
}
