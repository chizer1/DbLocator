using DbLocator.Db;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Features.DatabaseTypes;

internal record DeleteDatabaseTypeCommand(byte DatabaseTypeId);

internal sealed class DeleteDatabaseTypeCommandValidator
    : AbstractValidator<DeleteDatabaseTypeCommand>
{
    internal DeleteDatabaseTypeCommandValidator()
    {
        RuleFor(x => x.DatabaseTypeId).NotEmpty().WithMessage("Database Type Id is required.");
    }
}

internal class DeleteDatabaseType(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    IDistributedCache cache
)
{
    internal async Task Handle(DeleteDatabaseTypeCommand command)
    {
        await new DeleteDatabaseTypeCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseType =
            await dbContext
                .Set<DatabaseTypeEntity>()
                .FirstOrDefaultAsync(dt => dt.DatabaseTypeId == command.DatabaseTypeId)
            ?? throw new KeyNotFoundException(
                $"Database Type Id '{command.DatabaseTypeId}' not found."
            );

        if (
            await dbContext
                .Set<DatabaseEntity>()
                .AnyAsync(d => d.DatabaseTypeId == command.DatabaseTypeId)
        )
            throw new InvalidOperationException(
                $"Database Type '{databaseType.DatabaseTypeName}' is attached to a database, please remove the database first if you want to delete this database type."
            );

        dbContext.Remove(databaseType);
        await dbContext.SaveChangesAsync();

        cache?.Remove("databaseTypes");
    }
}
