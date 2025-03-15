using DbLocator.Db;
using DbLocator.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers;

internal class GetDatabaseServersQuery { }

internal sealed class GetDatabaseServersQueryValidator : AbstractValidator<GetDatabaseServersQuery>
{
    internal GetDatabaseServersQueryValidator() { }
}

internal class GetDatabaseServers(IDbContextFactory<DbLocatorContext> dbContextFactory)
{
    internal async Task<List<DatabaseServer>> Handle(GetDatabaseServersQuery query)
    {
        await new GetDatabaseServersQueryValidator().ValidateAndThrowAsync(query);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseServerEntities = await dbContext.Set<DatabaseServerEntity>().ToListAsync();

        return
        [
            .. databaseServerEntities.Select(ds => new DatabaseServer(
                ds.DatabaseServerId,
                ds.DatabaseServerName,
                ds.DatabaseServerIpaddress,
                ds.DatabaseServerHostName,
                ds.DatabaseServerFullyQualifiedDomainName,
                ds.IsLinkedServer
            ))
        ];
    }
}
