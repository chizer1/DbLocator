using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers.CreateDatabaseServer;

/// <summary>
/// Represents a command to create a new database server.
/// </summary>
internal record CreateDatabaseServerCommand(
    string Name,
    string HostName,
    string FullyQualifiedDomainName,
    string IpAddress,
    bool IsLinkedServer
);

/// <summary>
/// Validates the CreateDatabaseServerCommand.
/// </summary>
internal sealed class CreateDatabaseServerCommandValidator
    : AbstractValidator<CreateDatabaseServerCommand>
{
    public CreateDatabaseServerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Database Server Name is required.")
            .MaximumLength(50)
            .WithMessage("Database Server Name cannot be more than 50 characters.");

        RuleFor(x => x.HostName)
            .MaximumLength(50)
            .WithMessage("Host Name cannot be more than 50 characters.");

        RuleFor(x => x.FullyQualifiedDomainName)
            .MaximumLength(100)
            .WithMessage("Fully Qualified Domain Name cannot be more than 100 characters.");

        RuleFor(x => x.IpAddress)
            .MaximumLength(15)
            .WithMessage("IP Address cannot be more than 15 characters.");

        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrEmpty(x.HostName)
                || !string.IsNullOrEmpty(x.FullyQualifiedDomainName)
                || !string.IsNullOrEmpty(x.IpAddress)
            )
            .WithMessage(
                "At least one of Host Name, Fully Qualified Domain Name, or IP Address must be provided."
            );
    }
}

/// <summary>
/// Handles the CreateDatabaseServerCommand and creates a new database server.
/// </summary>
internal class CreateDatabaseServerHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    public async Task<int> Handle(CreateDatabaseServerCommand command)
    {
        await new CreateDatabaseServerCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        if (
            await dbContext
                .Set<DatabaseServerEntity>()
                .AnyAsync(c => c.DatabaseServerName == command.Name)
        )
            throw new ArgumentException($"Database Server '{command.Name}' already exists.");

        var databaseServer = new DatabaseServerEntity
        {
            DatabaseServerName = command.Name,
            DatabaseServerHostName = command.HostName,
            DatabaseServerFullyQualifiedDomainName = command.FullyQualifiedDomainName,
            DatabaseServerIpaddress = command.IpAddress,
            IsLinkedServer = command.IsLinkedServer
        };

        await dbContext.Set<DatabaseServerEntity>().AddAsync(databaseServer);
        await dbContext.SaveChangesAsync();

        cache?.Remove("databaseServers");

        return databaseServer.DatabaseServerId;
    }
}
