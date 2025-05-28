using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers.UpdateDatabaseServerName;

/// <summary>
/// Represents a command to update a database server's name.
/// </summary>
internal record UpdateDatabaseServerNameCommand(int DatabaseServerId, string Name);

/// <summary>
/// Validates the UpdateDatabaseServerNameCommand.
/// </summary>
internal sealed class UpdateDatabaseServerNameCommandValidator
    : AbstractValidator<UpdateDatabaseServerNameCommand>
{
    public UpdateDatabaseServerNameCommandValidator()
    {
        RuleFor(x => x.DatabaseServerId).GreaterThan(0);
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Database Server Name is required.")
            .MaximumLength(50)
            .WithMessage("Database Server Name cannot be more than 50 characters.");
    }
}

/// <summary>
/// Handles the UpdateDatabaseServerNameCommand and updates a database server's name.
/// </summary>
internal class UpdateDatabaseServerNameHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    public async Task Handle(UpdateDatabaseServerNameCommand command)
    {
        await new UpdateDatabaseServerNameCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseServer =
            await dbContext
                .Set<DatabaseServerEntity>()
                .FirstOrDefaultAsync(ds => ds.DatabaseServerId == command.DatabaseServerId)
            ?? throw new KeyNotFoundException(
                $"Database Server with ID {command.DatabaseServerId} not found."
            );

        if (
            await dbContext
                .Set<DatabaseServerEntity>()
                .AnyAsync(ds =>
                    ds.DatabaseServerName == command.Name
                    && ds.DatabaseServerId != command.DatabaseServerId
                )
        )
            throw new ArgumentException($"Database Server '{command.Name}' already exists.");

        databaseServer.DatabaseServerName = command.Name;
        dbContext.Set<DatabaseServerEntity>().Update(databaseServer);
        await dbContext.SaveChangesAsync();

        cache?.Remove("databaseServers");
        cache?.Remove($"databaseServer-id-{command.DatabaseServerId}");
    }
}
