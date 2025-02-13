using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Databases;

internal record AddDatabaseCommand(
    string DatabaseName,
    string DatabaseUser,
    string DatabaseUserPassword,
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

        RuleFor(x => x.DatabaseServerId).NotEmpty().WithMessage("Database Server Id is required.");
        RuleFor(x => x.DatabaseTypeId).NotEmpty().WithMessage("Database Type Id is required.");
        RuleFor(x => x.DatabaseStatus).IsInEnum().WithMessage("Database Status is required.");

        // optional parameter
        RuleFor(x => x.DatabaseUserPassword)
            .MinimumLength(10)
            .WithMessage("Database User Password must be at least 10 characters long.")
            .Matches(@"[A-Z]")
            .WithMessage("Database User Password must contain at least one uppercase letter.")
            .Matches(@"[0-9]")
            .WithMessage("Database User Password must contain at least one number.")
            .Matches(@"[\W_]")
            .WithMessage("Database User Password must contain at least one special character.")
            .MaximumLength(50);

        // optional parameter
        RuleFor(x => x.DatabaseUser)
            .MaximumLength(50)
            .WithMessage("Database User cannot be more than 50 characters.")
            .Matches(@"^\S*$")
            .WithMessage("Database User cannot contain spaces.");
    }
}

internal class AddDatabase(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    Encryption encryption
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

        var commands = new List<string>();

        if (command.CreateDatabase)
        {
            // check if SQL Server user exists as well

            commands.Add($"create database {command.DatabaseName}");

            commands.AddRange(
                [
                    $"create login {command.DatabaseUser} with password = '{command.DatabaseUserPassword}'",
                    $"use {command.DatabaseName}; create user {command.DatabaseUser} for login {command.DatabaseUser}",
                ]
            );
        }

        var database = new DatabaseEntity
        {
            DatabaseName = command.DatabaseName,
            DatabaseUser = command.DatabaseUser,
            DatabaseUserPassword = encryption.Encrypt(command.DatabaseUserPassword),
            DatabaseServerId = command.DatabaseServerId,
            DatabaseTypeId = command.DatabaseTypeId,
            DatabaseStatusId = (byte)command.DatabaseStatus,
            UseTrustedConnection = command.UseTrustedConnection,
        };

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
