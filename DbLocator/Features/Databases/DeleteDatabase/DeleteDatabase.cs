#nullable enable

using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Databases.DeleteDatabase;

internal record DeleteDatabaseCommand(int Id, bool AffectDatabase = true);

internal sealed class DeleteDatabaseCommandValidator : AbstractValidator<DeleteDatabaseCommand>
{
    internal DeleteDatabaseCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Database ID must be greater than zero");
    }
}

internal class DeleteDatabaseHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task Handle(
        DeleteDatabaseCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await new DeleteDatabaseCommandValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var database =
            await dbContext
                .Set<DatabaseEntity>()
                .Include(d => d.DatabaseServer)
                .FirstOrDefaultAsync(d => d.DatabaseId == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Database with ID {request.Id} not found.");

        var hasConnections = await dbContext
            .Set<ConnectionEntity>()
            .AnyAsync(c => c.DatabaseId == request.Id, cancellationToken);

        if (hasConnections)
        {
            throw new InvalidOperationException(
                $"Cannot delete database '{database.DatabaseName}' because it has existing connections."
            );
        }

        if (request.AffectDatabase)
        {
            var dbName = Sql.SanitizeSqlIdentifier(database.DatabaseName);
            await Sql.ExecuteSqlCommandAsync(
                dbContext,
                $"drop database [{dbName}]",
                database.DatabaseServer.IsLinkedServer,
                database.DatabaseServer.DatabaseServerHostName
                    ?? database.DatabaseServer.DatabaseServerName
            );
        }

        dbContext.Databases.Remove(database);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (_cache != null)
        {
            await _cache.Remove("databases");
            await _cache.Remove($"database-id-{request.Id}");
        }
    }
}
