#nullable enable

using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers.UpdateDatabaseServerNetwork;

internal record UpdateDatabaseServerNetworkCommand(
    int DatabaseServerId,
    string HostName,
    string FullyQualifiedDomainName,
    string IpAddress
);

internal sealed class UpdateDatabaseServerNetworkCommandValidator
    : AbstractValidator<UpdateDatabaseServerNetworkCommand>
{
    internal UpdateDatabaseServerNetworkCommandValidator()
    {
        RuleFor(x => x.DatabaseServerId)
            .GreaterThan(0)
            .WithMessage("Database Server Id must be greater than 0.");

        RuleFor(x => x.HostName)
            .MaximumLength(255)
            .WithMessage("Host Name cannot be more than 255 characters.");

        RuleFor(x => x.FullyQualifiedDomainName)
            .MaximumLength(100)
            .WithMessage("Fully Qualified Domain Name cannot be more than 100 characters.")
            .Matches(@"^(?!-)[A-Za-z0-9-]{1,63}(?<!-)(\.[A-Za-z0-9-]{1,63})*\.[A-Za-z]{2,}$")
            .When(x => !string.IsNullOrEmpty(x.FullyQualifiedDomainName))
            .WithMessage(
                "FQDN must be a valid domain name format (e.g., example.com, sub.example.com)"
            );

        RuleFor(x => x.IpAddress)
            .MaximumLength(15)
            .WithMessage("IP Address cannot be more than 15 characters.");
    }
}

internal class UpdateDatabaseServerNetworkHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task Handle(
        UpdateDatabaseServerNetworkCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await new UpdateDatabaseServerNetworkCommandValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        if (!string.IsNullOrEmpty(request.FullyQualifiedDomainName))
        {
            var fqdnRegex = new System.Text.RegularExpressions.Regex(
                @"^(?!-)[A-Za-z0-9-]{1,63}(?<!-)(\.[A-Za-z0-9-]{1,63})*\.[A-Za-z]{2,}$"
            );
            if (!fqdnRegex.IsMatch(request.FullyQualifiedDomainName))
            {
                throw new ValidationException(
                    "FQDN must be a valid domain name format (e.g., example.com, sub.example.com)"
                );
            }
        }

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

        if (!string.IsNullOrEmpty(request.HostName))
            databaseServer.DatabaseServerHostName = request.HostName;

        if (!string.IsNullOrEmpty(request.FullyQualifiedDomainName))
            databaseServer.DatabaseServerFullyQualifiedDomainName =
                request.FullyQualifiedDomainName;

        if (!string.IsNullOrEmpty(request.IpAddress))
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
