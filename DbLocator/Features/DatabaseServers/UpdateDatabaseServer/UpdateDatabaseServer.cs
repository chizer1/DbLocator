#nullable enable

using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers.UpdateDatabaseServer;

internal record UpdateDatabaseServerCommand(
    int DatabaseServerId,
    string? Name,
    string? HostName,
    string? FullyQualifiedDomainName,
    string? IpAddress,
    bool? IsLinkedServer
);

internal sealed class UpdateDatabaseServerCommandValidator
    : AbstractValidator<UpdateDatabaseServerCommand>
{
    internal UpdateDatabaseServerCommandValidator()
    {
        RuleFor(x => x.DatabaseServerId)
            .GreaterThan(0)
            .WithMessage("Database Server Id must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .When(x => x.Name != null)
            .WithMessage("Database Server Name is required.")
            .MaximumLength(50)
            .When(x => x.Name != null)
            .WithMessage("Database Server Name cannot be more than 50 characters.");

        RuleFor(x => x.HostName)
            .MaximumLength(50)
            .When(x => x.HostName != null)
            .WithMessage("Host Name cannot be more than 50 characters.");

        RuleFor(x => x.FullyQualifiedDomainName)
            .MaximumLength(100)
            .When(x => x.FullyQualifiedDomainName != null)
            .WithMessage("Fully Qualified Domain Name cannot be more than 100 characters.")
            .Matches(@"^(?!-)[A-Za-z0-9-]{1,63}(?<!-)(\.[A-Za-z0-9-]{1,63})*\.[A-Za-z]{2,}$")
            .When(x => !string.IsNullOrEmpty(x.FullyQualifiedDomainName))
            .WithMessage(
                "FQDN must be a valid domain name format (e.g., example.com, sub.example.com)"
            );

        RuleFor(x => x.IpAddress)
            .MaximumLength(15)
            .When(x => x.IpAddress != null)
            .WithMessage("IP Address cannot be more than 15 characters.");

        RuleFor(x => x)
            .Must(x =>
                x.Name != null
                || x.HostName != null
                || x.FullyQualifiedDomainName != null
                || x.IpAddress != null
                || x.IsLinkedServer.HasValue
            )
            .WithMessage("At least one field must be provided for update");
    }
}

internal class UpdateDatabaseServerHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task Handle(
        UpdateDatabaseServerCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await new UpdateDatabaseServerCommandValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var server =
            await dbContext
                .Set<DatabaseServerEntity>()
                .FirstOrDefaultAsync(s => s.DatabaseServerId == request.DatabaseServerId, cancellationToken)
            ?? throw new KeyNotFoundException($"Database server with ID {request.DatabaseServerId} not found.");

        // Check if any changes are provided
        if (
            request.Name == null
            && request.HostName == null
            && request.IpAddress == null
            && request.FullyQualifiedDomainName == null
            && request.IsLinkedServer == null
        )
        {
            throw new InvalidOperationException("At least one field must be provided for update");
        }

        // Check for duplicate properties
        if (request.Name != null && request.Name != server.DatabaseServerName)
        {
            var existingServer = await dbContext
                .Set<DatabaseServerEntity>()
                .FirstOrDefaultAsync(
                    s => s.DatabaseServerName == request.Name,
                    cancellationToken
                );
            if (existingServer != null)
            {
                throw new InvalidOperationException(
                    $"Database Server Name '{request.Name}' already exists"
                );
            }
        }

        if (request.HostName != null && request.HostName != server.DatabaseServerHostName)
        {
            var existingServer = await dbContext
                .Set<DatabaseServerEntity>()
                .FirstOrDefaultAsync(
                    s => s.DatabaseServerHostName == request.HostName,
                    cancellationToken
                );
            if (existingServer != null)
            {
                throw new InvalidOperationException(
                    $"Database server with host name \"{request.HostName}\" already exists"
                );
            }
        }

        if (request.FullyQualifiedDomainName != null
            && request.FullyQualifiedDomainName != server.DatabaseServerFullyQualifiedDomainName)
        {
            var existingServer = await dbContext
                .Set<DatabaseServerEntity>()
                .FirstOrDefaultAsync(
                    s => s.DatabaseServerFullyQualifiedDomainName == request.FullyQualifiedDomainName,
                    cancellationToken
                );
            if (existingServer != null)
            {
                throw new InvalidOperationException(
                    $"Database server with FQDN \"{request.FullyQualifiedDomainName}\" already exists"
                );
            }
        }

        if (request.IpAddress != null && request.IpAddress != server.DatabaseServerIpaddress)
        {
            var existingServer = await dbContext
                .Set<DatabaseServerEntity>()
                .FirstOrDefaultAsync(
                    s => s.DatabaseServerIpaddress == request.IpAddress,
                    cancellationToken
                );
            if (existingServer != null)
            {
                throw new InvalidOperationException(
                    $"Database server with IP address '{request.IpAddress}' already exists"
                );
            }
        }

        // Update server properties
        if (request.Name != null)
            server.DatabaseServerName = request.Name;
        if (request.HostName != null)
            server.DatabaseServerHostName = request.HostName;
        if (request.IpAddress != null)
            server.DatabaseServerIpaddress = request.IpAddress;
        if (request.FullyQualifiedDomainName != null)
            server.DatabaseServerFullyQualifiedDomainName = request.FullyQualifiedDomainName;
        if (request.IsLinkedServer.HasValue)
            server.IsLinkedServer = request.IsLinkedServer.Value;

        await dbContext.SaveChangesAsync(cancellationToken);

        if (_cache != null)
        {
            await _cache.Remove("database-servers");
            await _cache.Remove($"database-server-id-{request.DatabaseServerId}");
        }
    }
}
