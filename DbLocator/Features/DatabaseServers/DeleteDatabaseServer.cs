using DbLocator.Db;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers;

internal record DeleteDatabaseServerCommand(int DatabaseServerId);

internal sealed class DeleteDatabaseServerCommandValidator
    : AbstractValidator<DeleteDatabaseServerCommand>
{
    internal DeleteDatabaseServerCommandValidator()
    {
        RuleFor(x => x.DatabaseServerId).NotEmpty().WithMessage("Database Server Id is required.");
    }
}

internal class DeleteDatabaseServer(IDbContextFactory<DbLocatorContext> dbContextFactory)
{
    internal async Task Handle(DeleteDatabaseServerCommand command)
    {
        await new DeleteDatabaseServerCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseServer =
            await dbContext
                .Set<DatabaseServerEntity>()
                .FirstOrDefaultAsync(ds => ds.DatabaseServerId == command.DatabaseServerId)
            ?? throw new KeyNotFoundException(
                $"Database Server Id '{command.DatabaseServerId}' not found."
            );

        if (
            await dbContext
                .Set<DatabaseEntity>()
                .AnyAsync(d => d.DatabaseServerId == command.DatabaseServerId)
        )
            throw new InvalidOperationException(
                $"Cannot delete Database Server '{databaseServer.DatabaseServerName}' because there are databases associated with it."
            );

        dbContext.Remove(databaseServer);
        await dbContext.SaveChangesAsync();
    }
}
