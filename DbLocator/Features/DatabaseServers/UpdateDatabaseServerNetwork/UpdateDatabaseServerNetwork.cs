using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers.UpdateDatabaseServerNetwork;

/// <summary>
/// Represents a command to update a database server's network information.
/// </summary>
internal record UpdateDatabaseServerNetworkCommand(
    int DatabaseServerId,
    string FullyQualifiedDomainName,
    string IpAddress
);

/// <summary>
/// Validates the UpdateDatabaseServerNetworkCommand.
/// </summary>
internal sealed class UpdateDatabaseServerNetworkCommandValidator
    : AbstractValidator<UpdateDatabaseServerNetworkCommand>
{
    public UpdateDatabaseServerNetworkCommandValidator()
    {
        RuleFor(x => x.DatabaseServerId).GreaterThan(0);
        RuleFor(x => x.FullyQualifiedDomainName)
            .MaximumLength(100)
            .WithMessage("Fully Qualified Domain Name cannot be more than 100 characters.");
        RuleFor(x => x.IpAddress)
            .MaximumLength(15)
            .WithMessage("IP Address cannot be more than 15 characters.");
    }
}

/// <summary>
/// Handles the UpdateDatabaseServerNetworkCommand and updates a database server's network information.
/// </summary>
internal class UpdateDatabaseServerNetworkHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    public async Task Handle(UpdateDatabaseServerNetworkCommand command)
    {
        await new UpdateDatabaseServerNetworkCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseServer =
            await dbContext
                .Set<DatabaseServerEntity>()
                .FirstOrDefaultAsync(ds => ds.DatabaseServerId == command.DatabaseServerId)
            ?? throw new KeyNotFoundException(
                $"Database Server with ID {command.DatabaseServerId} not found."
            );

        if (!string.IsNullOrEmpty(command.FullyQualifiedDomainName))
            databaseServer.DatabaseServerFullyQualifiedDomainName =
                command.FullyQualifiedDomainName;

        if (!string.IsNullOrEmpty(command.IpAddress))
            databaseServer.DatabaseServerIpaddress = command.IpAddress;

        dbContext.Set<DatabaseServerEntity>().Update(databaseServer);
        await dbContext.SaveChangesAsync();

        cache?.Remove("databaseServers");
        cache?.Remove($"databaseServer-id-{command.DatabaseServerId}");
    }
}
