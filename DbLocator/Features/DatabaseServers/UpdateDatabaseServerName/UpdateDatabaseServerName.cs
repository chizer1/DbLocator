#nullable enable

using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseServers.UpdateDatabaseServerName;

internal record UpdateDatabaseServerNameCommand(int DatabaseServerId, string Name);

internal sealed class UpdateDatabaseServerNameCommandValidator
    : AbstractValidator<UpdateDatabaseServerNameCommand>
{
    internal UpdateDatabaseServerNameCommandValidator()
    {
        RuleFor(x => x.DatabaseServerId)
            .GreaterThan(0)
            .WithMessage("Database Server Id must be greater than 0.");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Database Server Name is required.")
            .MaximumLength(50)
            .WithMessage("Database Server Name cannot be more than 50 characters.");
    }
}

internal class UpdateDatabaseServerNameHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task Handle(
        UpdateDatabaseServerNameCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await new UpdateDatabaseServerNameCommandValidator().ValidateAndThrowAsync(
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
                $"Database server with ID {request.DatabaseServerId} not found"
            );

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
        dbContext.Set<DatabaseServerEntity>().Update(databaseServer);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (_cache != null)
        {
            await _cache.Remove("databaseServers");
            await _cache.Remove($"databaseServer-id-{request.DatabaseServerId}");
        }
    }
}
