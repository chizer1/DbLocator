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

        var databaseServer =
            await dbContext
                .Set<DatabaseServerEntity>()
                .FirstOrDefaultAsync(
                    ds => ds.DatabaseServerId == request.DatabaseServerId,
                    cancellationToken
                )
            ?? throw new KeyNotFoundException(
                $"Database Server with ID {request.DatabaseServerId} not found."
            );

        if (request.Name != null)
        {
            if (
                await dbContext
                    .Set<DatabaseServerEntity>()
                    .AnyAsync(
                        ds =>
                            ds.DatabaseServerName == request.Name
                            && ds.DatabaseServerId != request.DatabaseServerId,
                        cancellationToken
                    )
            )
                throw new InvalidOperationException(
                    $"Database server with name \"{request.Name}\" already exists"
                );
            databaseServer.DatabaseServerName = request.Name;
        }

        if (request.HostName != null)
        {
            if (
                !string.IsNullOrWhiteSpace(request.HostName)
                && await dbContext
                    .Set<DatabaseServerEntity>()
                    .AnyAsync(
                        ds =>
                            ds.DatabaseServerHostName == request.HostName
                            && ds.DatabaseServerId != request.DatabaseServerId,
                        cancellationToken
                    )
            )
                throw new InvalidOperationException(
                    $"Database server with host name \"{request.HostName}\" already exists"
                );
            databaseServer.DatabaseServerHostName = request.HostName;
        }

        if (request.FullyQualifiedDomainName != null)
        {
            if (
                !string.IsNullOrWhiteSpace(request.FullyQualifiedDomainName)
                && await dbContext
                    .Set<DatabaseServerEntity>()
                    .AnyAsync(
                        ds =>
                            ds.DatabaseServerFullyQualifiedDomainName
                                == request.FullyQualifiedDomainName
                            && ds.DatabaseServerId != request.DatabaseServerId,
                        cancellationToken
                    )
            )
                throw new InvalidOperationException(
                    $"Database server with FQDN \"{request.FullyQualifiedDomainName}\" already exists"
                );
            databaseServer.DatabaseServerFullyQualifiedDomainName =
                request.FullyQualifiedDomainName;
        }

        if (request.IpAddress != null)
            databaseServer.DatabaseServerIpaddress = request.IpAddress;

        dbContext.Set<DatabaseServerEntity>().Update(databaseServer);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (_cache != null)
        {
            await _cache.Remove("databaseServers");
            await _cache.Remove($"databaseServer-id-{request.DatabaseServerId}");
        }
    }
}
