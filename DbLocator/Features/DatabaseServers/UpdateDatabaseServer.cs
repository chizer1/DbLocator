using DbLocator.Db;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers;

internal record UpdateDatabaseServerCommand(
    int DatabaseServerId,
    string DatabaseServerName,
    string DatabaseServerIpAddress
);

internal sealed class UpdateDatabaseServerCommandValidator
    : AbstractValidator<UpdateDatabaseServerCommand>
{
    internal UpdateDatabaseServerCommandValidator()
    {
        RuleFor(x => x.DatabaseServerId).NotNull().WithMessage("Database Server Id is required.");

        RuleFor(x => x.DatabaseServerName)
            .NotEmpty()
            .WithMessage("Database Server Name is required.")
            .MaximumLength(50)
            .WithMessage("Database Server Name cannot be more than 50 characters.");

        RuleFor(x => x.DatabaseServerIpAddress)
            .NotEmpty()
            .WithMessage("Database Server IP Address is required.")
            .MaximumLength(50)
            .WithMessage("Database Server IP Address cannot be more than 50 characters.");
        // .Matches(
        //     @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$"
        // )
        // .WithMessage("Database Server IP Address must be a valid IP address.");
    }
}

internal class UpdateDatabaseServer(IDbContextFactory<DbLocatorContext> dbContextFactory)
{
    internal async Task Handle(UpdateDatabaseServerCommand command)
    {
        await new UpdateDatabaseServerCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseServer =
            await dbContext
                .Set<DatabaseServerEntity>()
                .FirstOrDefaultAsync(ds => ds.DatabaseServerId == command.DatabaseServerId)
            ?? throw new KeyNotFoundException(
                $"Database Server Id '{command.DatabaseServerId}' not found."
            );

        if (
            await dbContextFactory
                .CreateDbContext()
                .Set<DatabaseServerEntity>()
                .AnyAsync(ds => ds.DatabaseServerName == command.DatabaseServerName)
        )
            throw new InvalidOperationException(
                $"Database Server Name '{command.DatabaseServerName}' already exists."
            );

        if (
            await dbContextFactory
                .CreateDbContext()
                .Set<DatabaseServerEntity>()
                .AnyAsync(ds => ds.DatabaseServerIpaddress == command.DatabaseServerIpAddress)
        )
            throw new InvalidOperationException(
                $"Database Server IP Address '{command.DatabaseServerIpAddress}' already exists."
            );

        databaseServer.DatabaseServerName = command.DatabaseServerName;
        databaseServer.DatabaseServerIpaddress = command.DatabaseServerIpAddress;

        dbContext.Update(databaseServer);
        await dbContext.SaveChangesAsync();
    }
}
