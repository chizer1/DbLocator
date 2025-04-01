using DbLocator.Db;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Features.DatabaseTypes;

internal record AddDatabaseTypeCommand(string DatabaseTypeName);

internal sealed class AddDatabaseTypeCommandValidator : AbstractValidator<AddDatabaseTypeCommand>
{
    internal AddDatabaseTypeCommandValidator()
    {
        RuleFor(x => x.DatabaseTypeName)
            .NotEmpty()
            .WithMessage("Database Type Name is required.")
            .MaximumLength(20)
            .WithMessage("Database Type Name cannot be more than 20 characters.");
    }
}

internal class AddDatabaseType(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    IDistributedCache cache
)
{
    internal async Task<byte> Handle(AddDatabaseTypeCommand command)
    {
        await new AddDatabaseTypeCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        if (
            await dbContext
                .Set<DatabaseTypeEntity>()
                .AnyAsync(dt => dt.DatabaseTypeName == command.DatabaseTypeName)
        )
            throw new InvalidOperationException(
                $"Database Type '{command.DatabaseTypeName}' already exists."
            );

        var databaseType = new DatabaseTypeEntity { DatabaseTypeName = command.DatabaseTypeName };
        dbContext.Add(databaseType);
        await dbContext.SaveChangesAsync();

        cache?.Remove("databaseTypes");

        return databaseType.DatabaseTypeId;
    }
}
