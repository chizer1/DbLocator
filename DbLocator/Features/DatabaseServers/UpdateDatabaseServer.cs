using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

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
        RuleFor(x => x.DatabaseServerHostName)
            .MaximumLength(50)
            .WithMessage("Database Server Host Name cannot be more than 50 characters.")
            .Matches(@"^[a-zA-Z0-9][a-zA-Z0-9-.]*[a-zA-Z0-9]$")
            .WithMessage("Database Server Host Name must be a valid hostname.");

        RuleFor(x => x.DatabaseServerFullyQualifiedDomainName)
            .MaximumLength(50)
            .WithMessage("Database Server FQDN cannot be more than 50 characters.")
            .Matches(@"^([a-zA-Z0-9]([a-zA-Z0-9\-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$")
            .WithMessage("Database Server FQDN must be a valid domain name.");

        // RuleFor(x => x.DatabaseServerIpAddress)
        //     .MaximumLength(50)
        //     .WithMessage("Database Server IP Address cannot be more than 50 characters.")
        //     .When(x => !string.IsNullOrEmpty(x.DatabaseServerIpAddress))
        //     .Matches(
        //         @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$"
        //     )
        //     .WithMessage("Database Server IP Address must be a valid IPv4 address.");

        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrEmpty(x.DatabaseServerHostName)
                || !string.IsNullOrEmpty(x.DatabaseServerFullyQualifiedDomainName)
                || !string.IsNullOrEmpty(x.DatabaseServerIpAddress)
            )
            .WithMessage("At least one of Host Name, FQDN, or IP Address must be provided.");
    }
}

internal class UpdateDatabaseServer(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
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
}
