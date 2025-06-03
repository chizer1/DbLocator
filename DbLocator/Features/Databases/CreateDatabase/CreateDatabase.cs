#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Databases.CreateDatabase;

internal record CreateDatabaseCommand(
    string DatabaseName,
    int DatabaseServerId,
    int DatabaseTypeId,
    bool AffectDatabase = true,
    bool UseTrustedConnection = false,
    Status Status = Status.Active
);

internal sealed class CreateDatabaseCommandValidator : AbstractValidator<CreateDatabaseCommand>
{
    internal CreateDatabaseCommandValidator()
    {
        RuleFor(x => x.DatabaseName)
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

internal class CreateDatabaseHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<int> Handle(
        CreateDatabaseCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await new CreateDatabaseCommandValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        await using var dbContext = _dbContextFactory.CreateDbContext();

        // Validate that database server exists
        var server = await dbContext
            .Set<DatabaseServerEntity>()
            .FirstOrDefaultAsync(
                s => s.DatabaseServerId == request.DatabaseServerId,
                cancellationToken
            );

        if (server == null)
            throw new KeyNotFoundException(
                $"Database server with ID {request.DatabaseServerId} not found"
            );

        // Validate that database type exists
        var type = await dbContext
            .Set<DatabaseTypeEntity>()
            .FirstOrDefaultAsync(
                t => t.DatabaseTypeId == request.DatabaseTypeId,
                cancellationToken
            );

        if (type == null)
            throw new KeyNotFoundException(
                $"Database type with ID {request.DatabaseTypeId} not found"
            );

        var databaseEntity = new DatabaseEntity
        {
            DatabaseName = request.DatabaseName,
            DatabaseServerId = request.DatabaseServerId,
            DatabaseTypeId = (byte)request.DatabaseTypeId,
            DatabaseStatusId = (byte)request.Status,
            UseTrustedConnection = request.UseTrustedConnection
        };

        dbContext.Databases.Add(databaseEntity);
        await dbContext.SaveChangesAsync(cancellationToken);

        var savedEntity =
            await dbContext
                .Set<DatabaseEntity>()
                .Include(d => d.DatabaseType)
                .Include(d => d.DatabaseServer)
                .FirstOrDefaultAsync(
                    d => d.DatabaseId == databaseEntity.DatabaseId,
                    cancellationToken
                )
            ?? throw new KeyNotFoundException(
                $"Database with ID {databaseEntity.DatabaseId} not found"
            );

        if (request.AffectDatabase)
        {
            var dbName = Sql.SanitizeSqlIdentifier(request.DatabaseName);
            await Sql.ExecuteSqlCommandAsync(
                dbContext,
                $"create database [{dbName}]",
                savedEntity.DatabaseServer.IsLinkedServer,
                savedEntity.DatabaseServer.DatabaseServerHostName
            );
        }

        if (_cache != null)
            await _cache.Remove("databases");

        return savedEntity.DatabaseId;
    }
}
