using DbLocator.Db;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers;

internal record AddDatabaseServerCommand(string DatabaseServerName, string DatabaseServerIpAddress);

internal sealed class AddDatabaseServerCommandValidator
    : AbstractValidator<AddDatabaseServerCommand>
{
    internal AddDatabaseServerCommandValidator()
    {
        RuleFor(x => x.DatabaseServerName)
            .NotEmpty()
            .WithMessage("Database Server Name is required.")
            .MaximumLength(50)
            .WithMessage("Database Server Name cannot be more than 50 characters.");

        RuleFor(x => x.DatabaseServerIpAddress)
            .NotEmpty()
            .WithMessage("Database Server IP Address is required.")
            .MaximumLength(50)
            .WithMessage("Database Server IP Address cannot be more than 50 characters.")
            .Matches(
                @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$"
            )
            .WithMessage("Database Server IP Address must be a valid IP address.");
    }
}

internal class AddDatabaseServer(IDbContextFactory<DbLocatorContext> dbContextFactory)
{
    internal async Task<int> Handle(AddDatabaseServerCommand command)
    {
        await new AddDatabaseServerCommandValidator().ValidateAndThrowAsync(command);

        if (
            await dbContextFactory
                .CreateDbContext()
                .Set<DatabaseServerEntity>()
                .AnyAsync(ds => ds.DatabaseServerName == command.DatabaseServerName)
        )
            throw new InvalidOperationException(
                $"Database Server Name '{command.DatabaseServerName}' already exists."
            );

        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseServer = new DatabaseServerEntity
        {
            DatabaseServerName = command.DatabaseServerName,
            DatabaseServerIpaddress = command.DatabaseServerIpAddress,
        };

        dbContext.Add(databaseServer);
        await dbContext.SaveChangesAsync();

        return databaseServer.DatabaseServerId;
    }
}
