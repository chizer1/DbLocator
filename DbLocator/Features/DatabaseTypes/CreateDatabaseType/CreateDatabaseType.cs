#nullable enable

using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseTypes.CreateDatabaseType;

internal record CreateDatabaseTypeCommand(string DatabaseTypeName);

internal sealed class CreateDatabaseTypeCommandValidator
    : AbstractValidator<CreateDatabaseTypeCommand>
{
    internal CreateDatabaseTypeCommandValidator()
    {
        RuleFor(x => x.DatabaseTypeName)
            .NotEmpty()
            .WithMessage("Database type name is required")
            .MaximumLength(20)
            .WithMessage("Database type name cannot be more than 20 characters");
    }
}

internal class CreateDatabaseTypeHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<byte> Handle(
        CreateDatabaseTypeCommand request,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(request.DatabaseTypeName))
        {
            throw new ArgumentException("Database type name is required");
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var existingType = await dbContext
            .Set<DatabaseTypeEntity>()
            .FirstOrDefaultAsync(
                dt => dt.DatabaseTypeName == request.DatabaseTypeName,
                cancellationToken
            );

        if (existingType != null)
        {
            throw new InvalidOperationException(
                $"Database type with name '{request.DatabaseTypeName}' already exists"
            );
        }

        var databaseType = new DatabaseTypeEntity { DatabaseTypeName = request.DatabaseTypeName };

        dbContext.Set<DatabaseTypeEntity>().Add(databaseType);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (_cache != null)
        {
            await _cache.Remove("database-types");
        }

        return databaseType.DatabaseTypeId;
    }
}
