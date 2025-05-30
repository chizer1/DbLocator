#nullable enable

using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers.DeleteDatabaseServer;

internal record DeleteDatabaseServerCommand(int DatabaseServerId);

internal sealed class DeleteDatabaseServerCommandValidator
    : AbstractValidator<DeleteDatabaseServerCommand>
{
    internal DeleteDatabaseServerCommandValidator()
    {
        RuleFor(x => x.DatabaseServerId)
            .GreaterThan(0)
            .WithMessage("Database Server Id must be greater than 0.");
    }
}

internal class DeleteDatabaseServerHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task Handle(
        DeleteDatabaseServerCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await new DeleteDatabaseServerCommandValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var databaseServer =
            await dbContext
                .Set<DatabaseServerEntity>()
                .FirstOrDefaultAsync(
                    ds => ds.DatabaseServerId == request.DatabaseServerId,
                    cancellationToken
                )
            ?? throw new KeyNotFoundException(
                $"Database server with ID {request.DatabaseServerId} not found"
            );

        if (
            await dbContext
                .Set<DatabaseEntity>()
                .AnyAsync(d => d.DatabaseServerId == request.DatabaseServerId, cancellationToken)
        )
            throw new InvalidOperationException(
                $"Cannot delete database server '{databaseServer.DatabaseServerName}' because it is in use by one or more databases"
            );

        dbContext.Set<DatabaseServerEntity>().Remove(databaseServer);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (_cache != null)
        {
            await _cache.Remove("databaseServers");
            await _cache.Remove($"databaseServer-id-{request.DatabaseServerId}");
        }
    }
}
