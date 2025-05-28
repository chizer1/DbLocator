#nullable enable

using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseTypes.DeleteDatabaseType;

internal record DeleteDatabaseTypeCommand(byte DatabaseTypeId);

internal sealed class DeleteDatabaseTypeCommandValidator
    : AbstractValidator<DeleteDatabaseTypeCommand>
{
    internal DeleteDatabaseTypeCommandValidator()
    {
        RuleFor(x => x.DatabaseTypeId).NotEmpty().WithMessage("Database Type Id is required.");
    }
}

internal class DeleteDatabaseTypeHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task Handle(
        DeleteDatabaseTypeCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await new DeleteDatabaseTypeCommandValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var databaseType =
            await dbContext
                .Set<DatabaseTypeEntity>()
                .FirstOrDefaultAsync(
                    dt => dt.DatabaseTypeId == request.DatabaseTypeId,
                    cancellationToken
                )
            ?? throw new KeyNotFoundException(
                $"Database type with ID {request.DatabaseTypeId} not found"
            );

        if (
            await dbContext
                .Set<DatabaseEntity>()
                .AnyAsync(d => d.DatabaseTypeId == request.DatabaseTypeId, cancellationToken)
        )
            throw new InvalidOperationException(
                $"Cannot delete database type \"{databaseType.DatabaseTypeName}\" because it is in use by one or more databases"
            );

        dbContext.Remove(databaseType);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (_cache != null)
        {
            await _cache.Remove("databaseTypes");
            await _cache.Remove("connections");
            await _cache.TryClearConnectionStringFromCache(
                databaseTypeId: databaseType.DatabaseTypeId
            );
        }
    }
}

#nullable disable
