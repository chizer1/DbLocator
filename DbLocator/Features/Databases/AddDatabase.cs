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
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Database Name can only contain letters, numbers, and underscores.");

        RuleFor(x => x.DatabaseServerId).NotEmpty().WithMessage("Database Server Id is required.");
        RuleFor(x => x.DatabaseTypeId).NotEmpty().WithMessage("Database Type Id is required.");
        RuleFor(x => x.DatabaseStatus).IsInEnum().WithMessage("Database Status is required.");

        RuleFor(x => x.DatabaseUser)
            .NotEmpty()
            .WithMessage("Database User is required.")
            .MaximumLength(50)
            .WithMessage("Database User cannot be more than 50 characters.")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Database User can only contain letters, numbers, and underscores.");

        RuleFor(x => x.DatabaseUserPassword)
            .NotEmpty()
            .WithMessage("Database User Password is required.")
            .MinimumLength(8)
            .WithMessage("Database User Password must be at least 8 characters long.")
            .Matches(@"[A-Z]")
            .WithMessage("Database User Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]")
            .WithMessage("Database User Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]")
            .WithMessage("Database User Password must contain at least one number.")
            .Matches(@"[\W_]")
            .WithMessage("Database User Password must contain at least one special character.")
            .MaximumLength(50)
            .WithMessage("Database User Password cannot be more than 50 characters.");
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

        if (command.CreateDatabase)
        {
            var commands = new List<string>();
            commands.AddRange(
                [
                    $"create database {command.DatabaseName}",
                    $"create login {command.DatabaseUser} with password = '{command.DatabaseUserPassword}'",
                    $"use {command.DatabaseName}; create user {command.DatabaseUser} for login {command.DatabaseUser}",
                ]
            );

            foreach (var commandText in commands)
            {
                using var cmd = dbContext.Database.GetDbConnection().CreateCommand();
                cmd.CommandText = commandText;
                await dbContext.Database.OpenConnectionAsync();
                await cmd.ExecuteNonQueryAsync();
            }
        }

        return database.DatabaseId;
    }
}
