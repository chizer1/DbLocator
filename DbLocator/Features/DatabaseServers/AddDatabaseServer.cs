using DbLocator.Db;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Features.DatabaseServers;

internal record AddDatabaseServerCommand(
    string DatabaseServerName,
    string DatabaseServerHostName,
    string DatabaseServerFullyQualifiedDomainName,
    string DatabaseServerIpAddress,
    bool IsLinkedServer
);

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

internal class AddDatabaseServer(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    IDistributedCache cache
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;

    internal async Task<int> Handle(AddDatabaseServerCommand command)
    {
        await new AddDatabaseServerCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = _dbContextFactory.CreateDbContext();

        await Validation(command, dbContext);

        var databaseServer = new DatabaseServerEntity
        {
            DatabaseServerName = command.DatabaseServerName,
            DatabaseServerIpaddress = string.IsNullOrEmpty(command.DatabaseServerIpAddress)
                ? null
                : command.DatabaseServerIpAddress,
            DatabaseServerHostName = string.IsNullOrEmpty(command.DatabaseServerHostName)
                ? null
                : command.DatabaseServerHostName,
            DatabaseServerFullyQualifiedDomainName = string.IsNullOrEmpty(
                command.DatabaseServerFullyQualifiedDomainName
            )
                ? null
                : command.DatabaseServerFullyQualifiedDomainName,
            IsLinkedServer = command.IsLinkedServer
        };

        dbContext.Add(databaseServer);
        await dbContext.SaveChangesAsync();

        cache?.Remove("databaseServers");

        return databaseServer.DatabaseServerId;
    }

    private static async Task Validation(
        AddDatabaseServerCommand command,
        DbLocatorContext dbContext
    )
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

        if (
            await dbContext
                .Set<DatabaseServerEntity>()
                .AnyAsync(ds => ds.DatabaseServerName == command.DatabaseServerName)
        )
        {
            throw new InvalidOperationException(
                $"Database Server Name '{command.DatabaseServerName}' already exists."
            );
        }

        if (
            !string.IsNullOrEmpty(command.DatabaseServerHostName)
            && await dbContext
                .Set<DatabaseServerEntity>()
                .AnyAsync(ds => ds.DatabaseServerHostName == command.DatabaseServerHostName)
        )
        {
            throw new InvalidOperationException(
                $"Database Server Host Name '{command.DatabaseServerHostName}' already exists."
            );
        }

        if (
            !string.IsNullOrEmpty(command.DatabaseServerFullyQualifiedDomainName)
            && await dbContext
                .Set<DatabaseServerEntity>()
                .AnyAsync(ds =>
                    ds.DatabaseServerFullyQualifiedDomainName
                    == command.DatabaseServerFullyQualifiedDomainName
                )
        )
        {
            throw new InvalidOperationException(
                $"Database Server Fully Qualified Domain Name '{command.DatabaseServerFullyQualifiedDomainName}' already exists."
            );
        }

        if (
            !string.IsNullOrEmpty(command.DatabaseServerIpAddress)
            && await dbContext
                .Set<DatabaseServerEntity>()
                .AnyAsync(ds => ds.DatabaseServerIpaddress == command.DatabaseServerIpAddress)
        )
        {
            throw new InvalidOperationException(
                $"Database Server IP Address '{command.DatabaseServerIpAddress}' already exists."
            );
        }
    }
}
