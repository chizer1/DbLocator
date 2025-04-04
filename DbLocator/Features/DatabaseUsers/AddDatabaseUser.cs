using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Features.DatabaseUsers;

internal record AddDatabaseUserCommand(
    int DatabaseId,
    string UserName,
    string UserPassword,
    bool CreateUser
);

internal sealed class AddDatabaseUserCommandValidator : AbstractValidator<AddDatabaseUserCommand>
{
    internal AddDatabaseUserCommandValidator()
    {
        RuleFor(x => x.DatabaseId).NotEmpty().WithMessage("Database Id is required");

        RuleFor(x => x.UserName)
            .MaximumLength(50)
            .WithMessage("Database User cannot be more than 50 characters.")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("Database User can only contain letters, numbers, and underscores.");

        RuleFor(x => x.UserPassword)
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

internal class AddDatabaseUser(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    Encryption encryption,
    IDistributedCache cache
)
{
    internal async Task<int> Handle(AddDatabaseUserCommand command)
    {
        await new AddDatabaseUserCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        if (
            !await dbContext
                .Set<DatabaseEntity>()
                .AnyAsync(ds => ds.DatabaseId == command.DatabaseId)
        )
        {
            throw new KeyNotFoundException($"Database Id '{command.DatabaseId}' not found.");
        }

        if (
            (
                await dbContext
                    .Set<DatabaseUserEntity>()
                    .Where(u => u.UserName == command.UserName)
                    .AnyAsync()
            )
        )
        {
            throw new InvalidOperationException(
                $"DatabaseUser with name '{command.UserName}' already exists."
            );
        }

        var databaseUser = new DatabaseUserEntity()
        {
            DatabaseId = command.DatabaseId,
            UserName = command.UserName,
            UserPassword = encryption.Encrypt(command.UserPassword)
        };

        await dbContext.Set<DatabaseUserEntity>().AddAsync(databaseUser);
        await dbContext.SaveChangesAsync();

        if (command.CreateUser)
        {
            await CreateDatabaseUser(dbContext, command);
        }

        cache?.Remove("databaseUsers");

        return databaseUser.DatabaseUserId;
    }

    private static async Task CreateDatabaseUser(
        DbLocatorContext dbContext,
        AddDatabaseUserCommand command
    )
    {
        var database = await dbContext
            .Set<DatabaseEntity>()
            .Include(d => d.DatabaseServer)
            .FirstOrDefaultAsync(ds => ds.DatabaseId == command.DatabaseId);

        var commands = new List<string>
        {
            $"create login {command.UserName} with password = '{command.UserPassword}'",
            $"use {database.DatabaseName}; create user {command.UserName} for login {command.UserName}"
        };

        for (var i = 0; i < commands.Count; i++)
        {
            var commandText = commands[i];
            using var cmd = dbContext.Database.GetDbConnection().CreateCommand();

            if (database.DatabaseServer.IsLinkedServer)
            {
                commandText =
                    $"exec('{commandText.Replace("'", "''")}') at {database.DatabaseServer.DatabaseServerHostName};";
            }

            cmd.CommandText = commandText;
            await dbContext.Database.OpenConnectionAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
