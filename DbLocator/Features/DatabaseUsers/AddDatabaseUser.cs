using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUsers;

internal record AddDatabaseUserCommand(
    List<int> DatabaseIds,
    string UserName,
    string UserPassword,
    bool CreateUser
);

internal sealed class AddDatabaseUserCommandValidator : AbstractValidator<AddDatabaseUserCommand>
{
    internal AddDatabaseUserCommandValidator()
    {
        RuleFor(x => x.DatabaseIds).NotEmpty().WithMessage("At least one Database Id is required.");

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
    DbLocatorCache cache
)
{
    internal async Task<int> Handle(AddDatabaseUserCommand command)
    {
        await new AddDatabaseUserCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var nonExistentDatabaseIds = command
            .DatabaseIds.Where(id =>
                !dbContext.Set<DatabaseEntity>().Any(ds => ds.DatabaseId == id)
            )
            .ToList();

        if (nonExistentDatabaseIds.Any())
        {
            throw new KeyNotFoundException(
                $"The following Database Ids were not found: {string.Join(", ", nonExistentDatabaseIds)}"
            );
        }

        if (await dbContext.Set<DatabaseUserEntity>().AnyAsync(u => u.UserName == command.UserName))
        {
            throw new InvalidOperationException(
                $"DatabaseUser with name '{command.UserName}' already exists."
            );
        }

        var databaseUser = new DatabaseUserEntity()
        {
            UserName = command.UserName,
            UserPassword = encryption.Encrypt(command.UserPassword)
        };

        await dbContext.Set<DatabaseUserEntity>().AddAsync(databaseUser);
        await dbContext.SaveChangesAsync();

        var databaseUserId = databaseUser.DatabaseUserId;
        foreach (var databaseId in command.DatabaseIds)
        {
            var databaseUserDatabase = new DatabaseUserDatabaseEntity()
            {
                DatabaseUserId = databaseUserId,
                DatabaseId = databaseId
            };

            await dbContext.Set<DatabaseUserDatabaseEntity>().AddAsync(databaseUserDatabase);
            await dbContext.SaveChangesAsync();

            if (command.CreateUser)
            {
                // create login and user in the database server
                await CreateDatabaseUser(
                    dbContext,
                    databaseId,
                    command.UserName,
                    command.UserPassword
                );
            }
        }

        cache?.Remove("databaseUsers");

        return databaseUserId;
    }

    private static async Task CreateDatabaseUser(
        DbLocatorContext dbContext,
        int databaseId,
        string userName,
        string userPassword
    )
    {
        var database =
            await dbContext
                .Set<DatabaseEntity>()
                .Include(d => d.DatabaseServer)
                .FirstOrDefaultAsync(ds => ds.DatabaseId == databaseId)
            ?? throw new InvalidOperationException("Database not found.");

        var uName = Sql.EscapeForDynamicSql(Sql.SanitizeSqlIdentifier(userName));
        var uPassword = Sql.EscapeForDynamicSql(userPassword);
        var dbName = Sql.SanitizeSqlIdentifier(database.DatabaseName);

        var commands = new List<string>
        {
            $"create login [{userName}] with password = '{userPassword}'", // todo: login only needs to be created once then loop databases
            $"use [{dbName}]; create user [{userName}] for login [{userName}]"
        };

        foreach (var rawCommand in commands)
        {
            var commandText = rawCommand;

            if (database.DatabaseServer.IsLinkedServer)
            {
                var linkedServer = Sql.SanitizeSqlIdentifier(
                    database.DatabaseServer.DatabaseServerHostName
                );
                commandText =
                    $"exec('{Sql.EscapeForDynamicSql(commandText)}') at [{linkedServer}];";
            }

            using var cmd = dbContext.Database.GetDbConnection().CreateCommand();
            cmd.CommandText = commandText;

            await dbContext.Database.OpenConnectionAsync();
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
