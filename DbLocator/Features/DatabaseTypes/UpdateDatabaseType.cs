using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseTypes;

internal record UpdateDatabaseTypeCommand(byte DatabaseTypeId, string DatabaseTypeName);

internal sealed class UpdateDatabaseTypeCommandValidator
    : AbstractValidator<UpdateDatabaseTypeCommand>
{
    internal UpdateDatabaseTypeCommandValidator()
    {
        RuleFor(x => x.DatabaseTypeId).NotNull().WithMessage("Database Type Id is required.");
        RuleFor(x => x.DatabaseTypeName)
            .NotEmpty()
            .WithMessage("Database Type Name is required.")
            .MaximumLength(20)
            .WithMessage("Database Type Name cannot be more than 20 characters.");
    }
}

internal class UpdateDatabaseType(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    internal async Task Handle(UpdateDatabaseTypeCommand command)
    {
        await new UpdateDatabaseTypeCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseType =
            await dbContext
                .Set<DatabaseTypeEntity>()
                .FirstOrDefaultAsync(dt => dt.DatabaseTypeId == command.DatabaseTypeId)
            ?? throw new KeyNotFoundException(
                $"Database Type Id '{command.DatabaseTypeId}' not found."
            );

        // Check if the new name is already in use by another database type
        if (
            await dbContext
                .Set<DatabaseTypeEntity>()
                .AnyAsync(dt =>
                    dt.DatabaseTypeName == command.DatabaseTypeName
                    && dt.DatabaseTypeId != command.DatabaseTypeId
                )
        )
            throw new InvalidOperationException(
                $"Database Type '{command.DatabaseTypeName}' already exists."
            );

        databaseType.DatabaseTypeName = command.DatabaseTypeName;

        dbContext.Update(databaseType);
        await dbContext.SaveChangesAsync();

        cache?.Remove("databaseTypes");
        cache?.Remove("connections");
        cache?.TryClearConnectionStringFromCache(databaseTypeId: databaseType.DatabaseTypeId);
    }
}
