#nullable enable

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
    internal UpdateDatabaseServerNetworkCommandValidator()
    {
        RuleFor(x => x.DatabaseServerId)
            .GreaterThan(0)
            .WithMessage("Database Server Id must be greater than 0.");

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

#nullable disable
