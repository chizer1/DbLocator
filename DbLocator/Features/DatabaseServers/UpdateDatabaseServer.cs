using DbLocator.Db;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Features.DatabaseServers;

internal record UpdateDatabaseServerCommand(
    int DatabaseServerId,
    string DatabaseServerName,
    string DatabaseServerIpAddress,
    string DatabaseServerHostName,
    string DatabaseServerFullyQualifiedDomainName
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

        RuleFor(x => x.DatabaseServerHostName)
            .MaximumLength(50)
            .WithMessage("Database Server Host Name cannot be more than 50 characters.");

        RuleFor(x => x.DatabaseServerFullyQualifiedDomainName)
            .MaximumLength(50)
            .WithMessage(
                "Database Server Fully Qualified Domain Name cannot be more than 50 characters."
            );
        // todo: add ip regex

        RuleFor(x => x.DatabaseServerIpAddress)
            .MaximumLength(50)
            .WithMessage("Database Server IP Address cannot be more than 50 characters.");
        // todo: add domain regex
    }
}

internal class UpdateDatabaseServer(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    IDistributedCache cache
)
{
    internal async Task Handle(UpdateDatabaseServerCommand command)
    {
        await new UpdateDatabaseServerCommandValidator().ValidateAndThrowAsync(command);

        Validate(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseServer =
            await dbContext
                .Set<DatabaseServerEntity>()
                .FirstOrDefaultAsync(ds => ds.DatabaseServerId == command.DatabaseServerId)
            ?? throw new KeyNotFoundException(
                $"Database Server Id '{command.DatabaseServerId}' not found."
            );

        databaseServer.DatabaseServerName = command.DatabaseServerName;

        if (!string.IsNullOrEmpty(command.DatabaseServerHostName))
            databaseServer.DatabaseServerHostName = command.DatabaseServerHostName;
        if (!string.IsNullOrEmpty(command.DatabaseServerFullyQualifiedDomainName))
            databaseServer.DatabaseServerFullyQualifiedDomainName =
                command.DatabaseServerFullyQualifiedDomainName;
        if (!string.IsNullOrEmpty(command.DatabaseServerIpAddress))
            databaseServer.DatabaseServerIpaddress = command.DatabaseServerIpAddress;

        dbContext.Update(databaseServer);
        await dbContext.SaveChangesAsync();

        cache?.Remove("databaseServers");
    }

    private static void Validate(UpdateDatabaseServerCommand command)
    {
        if (
            string.IsNullOrEmpty(command.DatabaseServerHostName)
            && string.IsNullOrEmpty(command.DatabaseServerFullyQualifiedDomainName)
            && string.IsNullOrEmpty(command.DatabaseServerIpAddress)
        )
        {
            throw new InvalidOperationException(
                "At least one of the following fields must be provided: Database Server Host Name, Database Server Fully Qualified Domain Name, Database Server IP Address."
            );
        }
    }
}
