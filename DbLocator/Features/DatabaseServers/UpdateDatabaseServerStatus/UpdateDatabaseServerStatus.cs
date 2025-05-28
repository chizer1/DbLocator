using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers.UpdateDatabaseServerStatus;

/// <summary>
/// Represents a command to update a database server's linked server status.
/// </summary>
internal record UpdateDatabaseServerStatusCommand(int DatabaseServerId, bool IsLinkedServer);

/// <summary>
/// Validates the UpdateDatabaseServerStatusCommand.
/// </summary>
internal sealed class UpdateDatabaseServerStatusCommandValidator
    : AbstractValidator<UpdateDatabaseServerStatusCommand>
{
    public UpdateDatabaseServerStatusCommandValidator()
    {
        RuleFor(x => x.DatabaseServerId).GreaterThan(0);
    }
}

/// <summary>
/// Handles the UpdateDatabaseServerStatusCommand and updates a database server's linked server status.
/// </summary>
internal class UpdateDatabaseServerStatusHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache _cache = cache;

    public async Task Handle(UpdateDatabaseServerStatusCommand command)
    {
        await new UpdateDatabaseServerStatusCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var databaseServer =
            await dbContext
                .Set<DatabaseServerEntity>()
                .FirstOrDefaultAsync(ds => ds.DatabaseServerId == command.DatabaseServerId)
            ?? throw new KeyNotFoundException(
                $"Database Server with ID {command.DatabaseServerId} not found."
            );

        databaseServer.IsLinkedServer = command.IsLinkedServer;
        dbContext.Set<DatabaseServerEntity>().Update(databaseServer);
        await dbContext.SaveChangesAsync();

        _cache?.Remove("databaseServers");
        _cache?.Remove($"databaseServer-id-{command.DatabaseServerId}");
    }
}
