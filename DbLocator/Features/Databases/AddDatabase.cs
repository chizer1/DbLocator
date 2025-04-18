using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Databases;

internal record AddDatabaseCommand(
    string DatabaseName,
    int DatabaseServerId,
    byte DatabaseTypeId,
    Status DatabaseStatus,
    bool UseTrustedConnection,
    bool CreateDatabase
);

internal sealed class AddDatabaseCommandValidator : AbstractValidator<AddDatabaseCommand>
{
    internal AddDatabaseCommandValidator()
    {
        RuleFor(x => x.DatabaseName)
            .NotEmpty()
            .WithMessage("Database Name is required.")
            .MaximumLength(50)
            .WithMessage("Database Name cannot be more than 50 characters.")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Database Name can only contain letters, numbers, and underscores.");

        RuleFor(x => x.DatabaseServerId).NotEmpty().WithMessage("Database Server Id is required.");
        RuleFor(x => x.DatabaseTypeId).NotEmpty().WithMessage("Database Type Id is required.");
        RuleFor(x => x.DatabaseStatus).IsInEnum().WithMessage("Database Status is required.");
    }
}

internal class AddDatabase(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache cache
)
{
    internal async Task<int> Handle(AddDatabaseCommand command)
    {
        await new AddDatabaseCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        if (
            !await dbContext
                .Set<DatabaseServerEntity>()
                .AnyAsync(ds => ds.DatabaseServerId == command.DatabaseServerId)
        )
        {
            throw new KeyNotFoundException(
                $"Database Server Id '{command.DatabaseServerId}' not found."
            );
        }

        if (
            !await dbContext
                .Set<DatabaseTypeEntity>()
                .AnyAsync(dt => dt.DatabaseTypeId == command.DatabaseTypeId)
        )
        {
            throw new KeyNotFoundException(
                $"Database Type Id '{command.DatabaseTypeId}' not found."
            );
        }

        var database = new DatabaseEntity
        {
            DatabaseName = command.DatabaseName,
            DatabaseServerId = command.DatabaseServerId,
            DatabaseTypeId = command.DatabaseTypeId,
            DatabaseStatusId = (byte)command.DatabaseStatus,
            UseTrustedConnection = command.UseTrustedConnection
        };

        await dbContext.Set<DatabaseEntity>().AddAsync(database);
        await dbContext.SaveChangesAsync();

        if (command.CreateDatabase)
            await CreateDatabaseAsync(command, dbContext);

        cache?.Remove("databases");

        return database.DatabaseId;
    }

    private static async Task CreateDatabaseAsync(
        AddDatabaseCommand command,
        DbLocatorContext dbContext
    )
    {
        var databaseServer =
            await dbContext
                .Set<DatabaseServerEntity>()
                .FirstOrDefaultAsync(ds => ds.DatabaseServerId == command.DatabaseServerId)
            ?? throw new KeyNotFoundException("Database server not found.");

        var databaseName = Sql.SanitizeSqlIdentifier(command.DatabaseName);

        await Sql.ExecuteSqlCommandAsync(
            dbContext,
            $"create database [{databaseName}]",
            databaseServer.IsLinkedServer,
            databaseServer.DatabaseServerHostName
        );
    }
}
