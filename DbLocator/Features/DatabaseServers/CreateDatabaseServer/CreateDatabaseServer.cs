#nullable enable

using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers.CreateDatabaseServer;

internal record CreateDatabaseServerCommand(
    string Name,
    string HostName,
    string FullyQualifiedDomainName,
    string IpAddress,
    bool IsLinkedServer
);

internal sealed class CreateDatabaseServerCommandValidator
    : AbstractValidator<CreateDatabaseServerCommand>
{
    public CreateDatabaseServerCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Database server name is required")
            .MaximumLength(50)
            .WithMessage("Database server name cannot be more than 50 characters");

        RuleFor(x => x.HostName)
            .MaximumLength(50)
            .WithMessage("Host name cannot be more than 50 characters");

        RuleFor(x => x.FullyQualifiedDomainName)
            .MaximumLength(100)
            .WithMessage("Fully qualified domain name cannot be more than 100 characters")
            .Matches(@"^(?!-)[A-Za-z0-9-]{1,63}(?<!-)(\.[A-Za-z0-9-]{1,63})*\.[A-Za-z]{2,}$")
            .When(x => !string.IsNullOrEmpty(x.FullyQualifiedDomainName))
            .WithMessage(
                "FQDN must be a valid domain name format (e.g., example.com, sub.example.com)"
            );

        RuleFor(x => x.IpAddress)
            .MaximumLength(15)
            .WithMessage("IP address cannot be more than 15 characters");

        RuleFor(x => x)
            .Must(x =>
                !string.IsNullOrEmpty(x.HostName)
                || !string.IsNullOrEmpty(x.FullyQualifiedDomainName)
                || !string.IsNullOrEmpty(x.IpAddress)
            )
            .WithMessage(
                "At least one network identifier (host name, FQDN, or IP address) is required"
            );
    }
}

internal class CreateDatabaseServerHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<int> Handle(
        CreateDatabaseServerCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await new CreateDatabaseServerCommandValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        await using var dbContext = _dbContextFactory.CreateDbContext();

        if (
            await dbContext
                .Set<DatabaseServerEntity>()
                .AnyAsync(ds => ds.DatabaseServerName == request.Name, cancellationToken)
        )
            throw new InvalidOperationException(
                $"Database Server Name '{request.Name}' already exists"
            );
        if (
            !string.IsNullOrWhiteSpace(request.HostName)
            && await dbContext
                .Set<DatabaseServerEntity>()
                .AnyAsync(ds => ds.DatabaseServerHostName == request.HostName, cancellationToken)
        )
            throw new InvalidOperationException(
                $"Database server with host name \"{request.HostName}\" already exists"
            );
        if (
            !string.IsNullOrWhiteSpace(request.FullyQualifiedDomainName)
            && await dbContext
                .Set<DatabaseServerEntity>()
                .AnyAsync(
                    ds =>
                        ds.DatabaseServerFullyQualifiedDomainName
                        == request.FullyQualifiedDomainName,
                    cancellationToken
                )
        )
            throw new InvalidOperationException(
                $"Database server with FQDN \"{request.FullyQualifiedDomainName}\" already exists"
            );
        if (
            !string.IsNullOrWhiteSpace(request.IpAddress)
            && await dbContext
                .Set<DatabaseServerEntity>()
                .AnyAsync(ds => ds.DatabaseServerIpaddress == request.IpAddress, cancellationToken)
        )
            throw new InvalidOperationException(
                $"Database server with IP address '{request.IpAddress}' already exists"
            );

        var databaseServer = new DatabaseServerEntity
        {
            DatabaseServerName = request.Name,
            DatabaseServerHostName = request.HostName,
            DatabaseServerFullyQualifiedDomainName = request.FullyQualifiedDomainName,
            DatabaseServerIpaddress = request.IpAddress,
            IsLinkedServer = request.IsLinkedServer
        };

        await dbContext.Set<DatabaseServerEntity>().AddAsync(databaseServer, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (_cache != null)
            await _cache.Remove("databaseServers");

        return databaseServer.DatabaseServerId;
    }
}
