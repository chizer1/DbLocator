using DbLocator.Db;
using DbLocator.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Databases;

internal record AddDatabaseCommand(
    string DatabaseName,
    string DatabaseUser,
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
            .Matches(@"^\S*$")
            .WithMessage("Database Name cannot contain spaces.");
        RuleFor(x => x.DatabaseServerId).NotEmpty().WithMessage("Database Server Id  is required.");
        RuleFor(x => x.DatabaseTypeId).NotEmpty().WithMessage("Database Type Id is required.");
        RuleFor(x => x.DatabaseStatus).IsInEnum().WithMessage("Database Status is required.");
    }
}

internal class AddDatabase(IDbContextFactory<DbLocatorContext> dbContextFactory)
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
            throw new KeyNotFoundException(
                $"Database Server Id '{command.DatabaseServerId}' not found."
            );

        if (
            !await dbContext
                .Set<DatabaseTypeEntity>()
                .AnyAsync(dt => dt.DatabaseTypeId == command.DatabaseTypeId)
        )
            throw new KeyNotFoundException(
                $"Database Type Id '{command.DatabaseTypeId}' not found."
            );

        var database = new DatabaseEntity
        {
            DatabaseName = command.DatabaseName,
            DatabaseServerId = command.DatabaseServerId,
            DatabaseTypeId = command.DatabaseTypeId,
            DatabaseStatusId = (byte)command.DatabaseStatus,
            UseTrustedConnection = command.UseTrustedConnection,
        };

        var commands = new List<string>();

        if (command.CreateDatabase)
            commands.Add($"CREATE DATABASE {command.DatabaseName}");

        if (!command.UseTrustedConnection)
        {
            database.DatabaseUser = command.DatabaseUser;

            var password = Guid.NewGuid().ToString(); // should user be able to set password?
            database.DatabaseUserPassword = password; // encrypt here later?

            commands.AddRange(
                [
                    $"CREATE LOGIN {command.DatabaseUser} WITH PASSWORD = '{password}'",
                    $"USE {command.DatabaseName}; CREATE USER {command.DatabaseUser} FOR LOGIN {command.DatabaseUser}",
                ]
            );
        }

        await dbContext.Set<DatabaseEntity>().AddAsync(database);
        await dbContext.SaveChangesAsync();

        foreach (var commandText in commands)
        {
            using var cmd = dbContext.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = commandText;
            await dbContext.Database.OpenConnectionAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        return database.DatabaseId;
    }
}
