#nullable enable

using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseTypes.UpdateDatabaseType;

internal record UpdateDatabaseTypeCommand(byte DatabaseTypeId, string DatabaseTypeName);

internal sealed class UpdateDatabaseTypeCommandValidator
    : AbstractValidator<UpdateDatabaseTypeCommand>
{
    internal UpdateDatabaseTypeCommandValidator()
    {
        RuleFor(x => x.DatabaseTypeId).NotNull().WithMessage("Database type ID is required");

        RuleFor(x => x.DatabaseTypeName)
            .NotEmpty()
            .WithMessage("Database type name is required")
            .MaximumLength(20)
            .WithMessage("Database type name cannot be more than 20 characters");
    }
}

internal class UpdateDatabaseTypeHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task Handle(
        UpdateDatabaseTypeCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await new UpdateDatabaseTypeCommandValidator().ValidateAndThrowAsync(
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

        // Check if the new name is already in use by another database type
        if (
            await dbContext
                .Set<DatabaseTypeEntity>()
                .AnyAsync(
                    dt =>
                        dt.DatabaseTypeName == request.DatabaseTypeName
                        && dt.DatabaseTypeId != request.DatabaseTypeId,
                    cancellationToken
                )
        )
            throw new InvalidOperationException(
                $"Database type with name \"{request.DatabaseTypeName}\" already exists"
            );

        databaseType.DatabaseTypeName = request.DatabaseTypeName;

        dbContext.Update(databaseType);
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
