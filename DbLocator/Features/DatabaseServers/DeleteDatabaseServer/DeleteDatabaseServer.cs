using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers.DeleteDatabaseServer;

/// <summary>
/// Represents a command to delete a database server.
/// </summary>
internal record DeleteDatabaseServerCommand(int DatabaseServerId);

/// <summary>
/// Validates the DeleteDatabaseServerCommand.
/// </summary>
internal sealed class DeleteDatabaseServerCommandValidator
    : AbstractValidator<DeleteDatabaseServerCommand>
{
    internal DeleteDatabaseServerCommandValidator()
    {
        RuleFor(x => x.DatabaseServerId).GreaterThan(0);
    }
}

/// <summary>
/// Handles the DeleteDatabaseServerCommand and deletes a database server.
/// </summary>
internal class DeleteDatabaseServerHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache _cache = cache;

    public async Task Handle(DeleteDatabaseServerCommand command)
    {
        await new DeleteDatabaseServerCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var databaseServer =
            await dbContext
                .Set<DatabaseServerEntity>()
                .FirstOrDefaultAsync(ds => ds.DatabaseServerId == command.DatabaseServerId)
            ?? throw new KeyNotFoundException(
                $"Database Server with ID {command.DatabaseServerId} not found."
            );

        if (
            await dbContext
                .Set<DatabaseEntity>()
                .AnyAsync(d => d.DatabaseServerId == command.DatabaseServerId)
        )
            throw new InvalidOperationException(
                "Cannot delete database server because there are databases associated with it, please delete the databases first."
            );

        dbContext.Set<DatabaseServerEntity>().Remove(databaseServer);
        await dbContext.SaveChangesAsync();

        _cache?.Remove("databaseServers");
        _cache?.Remove($"databaseServer-id-{command.DatabaseServerId}");
    }
}
