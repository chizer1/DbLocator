#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Databases.UpdateDatabase;

internal record UpdateDatabaseCommand(
    int Id,
    string Name,
    int DatabaseServerId,
    int DatabaseTypeId,
    bool UseTrustedConnection = false,
    Status Status = Status.Active,
    bool AffectDatabase = true
);

internal sealed class UpdateDatabaseCommandValidator : AbstractValidator<UpdateDatabaseCommand>
{
    internal UpdateDatabaseCommandValidator()
    {
        RuleFor(x => x.Id).GreaterThan(0).WithMessage("Database ID must be greater than zero");

        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Database name is required")
            .MaximumLength(50)
            .WithMessage("Database name cannot be more than 50 characters");

        RuleFor(x => x.DatabaseServerId)
            .GreaterThan(0)
            .WithMessage("Database server ID must be greater than zero");

        RuleFor(x => x.DatabaseTypeId)
            .GreaterThan(0)
            .WithMessage("Database type ID must be greater than zero");

        RuleFor(x => x.Status).IsInEnum().WithMessage("Status must be a valid Status enum value");
    }
}

internal class UpdateDatabaseHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<Database> Handle(
        UpdateDatabaseCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await new UpdateDatabaseCommandValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        await using var dbContext = _dbContextFactory.CreateDbContext();

        // First verify that the database type exists
        var databaseType = await dbContext
            .Set<DatabaseTypeEntity>()
            .FirstOrDefaultAsync(dt => dt.DatabaseTypeId == request.DatabaseTypeId, cancellationToken)
            ?? throw new KeyNotFoundException($"Database type with ID {request.DatabaseTypeId} not found");

        var database =
            await dbContext
                .Set<DatabaseEntity>()
                .Include(d => d.DatabaseType)
                .Include(d => d.DatabaseServer)
                .FirstOrDefaultAsync(d => d.DatabaseId == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Database with ID {request.Id} not found");

        if (request.AffectDatabase && database.DatabaseName != request.Name)
        {
            var oldDbName = Sql.SanitizeSqlIdentifier(database.DatabaseName);
            var newDbName = Sql.SanitizeSqlIdentifier(request.Name);
            await Sql.ExecuteSqlCommandAsync(
                dbContext,
                $"alter database [{oldDbName}] modify name = [{newDbName}]",
                database.DatabaseServer.IsLinkedServer,
                database.DatabaseServer.DatabaseServerHostName ?? database.DatabaseServer.DatabaseServerName
            );
        }

        database.DatabaseName = request.Name;
        database.DatabaseServerId = request.DatabaseServerId;
        database.DatabaseTypeId = (byte)request.DatabaseTypeId;
        database.UseTrustedConnection = request.UseTrustedConnection;
        database.DatabaseStatusId = (byte)request.Status;

        await dbContext.SaveChangesAsync(cancellationToken);

        if (_cache != null)
        {
            await _cache.Remove("databases");
            await _cache.Remove($"database-id-{request.Id}");
        }

        return new Database(
            database.DatabaseId,
            database.DatabaseName,
            new DatabaseType(databaseType.DatabaseTypeId, databaseType.DatabaseTypeName),
            new DatabaseServer(
                database.DatabaseServerId,
                database.DatabaseServer.DatabaseServerName,
                database.DatabaseServer.DatabaseServerHostName,
                database.DatabaseServer.DatabaseServerIpaddress,
                database.DatabaseServer.DatabaseServerFullyQualifiedDomainName,
                database.DatabaseServer.IsLinkedServer
            ),
            (Status)database.DatabaseStatusId,
            database.UseTrustedConnection
        );
    }
}

#nullable disable
