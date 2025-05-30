#nullable enable

using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers.UpdateDatabaseServerStatus;

internal record UpdateDatabaseServerStatusCommand(int DatabaseServerId, bool IsLinkedServer);

internal sealed class UpdateDatabaseServerStatusCommandValidator
    : AbstractValidator<UpdateDatabaseServerStatusCommand>
{
    internal UpdateDatabaseServerStatusCommandValidator()
    {
        RuleFor(x => x.DatabaseServerId)
            .GreaterThan(0)
            .WithMessage("Database Server Id must be greater than 0.");
    }
}

internal class UpdateDatabaseServerStatusHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task Handle(
        UpdateDatabaseServerStatusCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await new UpdateDatabaseServerStatusCommandValidator().ValidateAndThrowAsync(
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

        databaseServer.IsLinkedServer = request.IsLinkedServer;
        dbContext.Set<DatabaseServerEntity>().Update(databaseServer);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (_cache != null)
        {
            await _cache.Remove("databaseServers");
            await _cache.Remove($"databaseServer-id-{request.DatabaseServerId}");
        }
    }
}
