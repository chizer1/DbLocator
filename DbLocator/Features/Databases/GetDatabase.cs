using DbLocator.Db;
using DbLocator.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Databases;

internal record GetDatabaseQuery(int DatabaseId);

internal sealed class GetDatabaseQueryValidator : AbstractValidator<GetDatabaseQuery>
{
    internal GetDatabaseQueryValidator()
    {
        RuleFor(x => x.DatabaseId)
            .NotEmpty()
            .WithMessage("Database Id is required.")
            .GreaterThan(0)
            .WithMessage("Database Id must be greater than 0.");
    }
}

internal class GetDatabase(IDbContextFactory<DbLocatorContext> dbContextFactory)
{
    public async Task<Database> Handle(GetDatabaseQuery query)
    {
        await new GetDatabaseQueryValidator().ValidateAndThrowAsync(query);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseEntity =
            await dbContext
                .Set<DatabaseEntity>()
                .Include(d => d.DatabaseServer)
                .Include(d => d.DatabaseType)
                .FirstOrDefaultAsync(d => d.DatabaseId == query.DatabaseId)
            ?? throw new KeyNotFoundException($"Database with ID {query.DatabaseId} not found.");

        return new Database(
            databaseEntity.DatabaseId,
            databaseEntity.DatabaseName,
            new DatabaseType(
                databaseEntity.DatabaseType.DatabaseTypeId,
                databaseEntity.DatabaseType.DatabaseTypeName
            ),
            new DatabaseServer(
                databaseEntity.DatabaseServer.DatabaseServerId,
                databaseEntity.DatabaseServer.DatabaseServerName,
                databaseEntity.DatabaseServer.DatabaseServerIpaddress,
                databaseEntity.DatabaseServer.DatabaseServerHostName,
                databaseEntity.DatabaseServer.DatabaseServerFullyQualifiedDomainName,
                databaseEntity.DatabaseServer.IsLinkedServer
            ),
            (Status)databaseEntity.DatabaseStatusId,
            databaseEntity.UseTrustedConnection
        );
    }
}
