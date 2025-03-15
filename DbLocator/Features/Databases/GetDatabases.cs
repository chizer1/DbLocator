using DbLocator.Db;
using DbLocator.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Databases;

internal class GetDatabasesQuery { }

internal sealed class GetDatabasesQueryValidator : AbstractValidator<GetDatabasesQuery>
{
    internal GetDatabasesQueryValidator() { }
}

internal class GetDatabases(IDbContextFactory<DbLocatorContext> dbContextFactory)
{
    public async Task<List<Database>> Handle(GetDatabasesQuery query)
    {
        await new GetDatabasesQueryValidator().ValidateAndThrowAsync(query);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseEntities = await dbContext
            .Set<DatabaseEntity>()
            .Include(d => d.DatabaseServer)
            .Include(d => d.DatabaseType)
            .ToListAsync();

        return
        [
            .. databaseEntities.Select(d => new Database(
                d.DatabaseId,
                d.DatabaseName,
                d.DatabaseUser,
                new DatabaseType(d.DatabaseType.DatabaseTypeId, d.DatabaseType.DatabaseTypeName),
                new DatabaseServer(
                    d.DatabaseServer.DatabaseServerId,
                    d.DatabaseServer.DatabaseServerName,
                    d.DatabaseServer.DatabaseServerIpaddress,
                    d.DatabaseServer.DatabaseServerHostName,
                    d.DatabaseServer.DatabaseServerFullyQualifiedDomainName,
                    d.DatabaseServer.IsLinkedServer
                ),
                (Status)d.DatabaseStatusId,
                d.UseTrustedConnection
            ))
        ];
    }
}
