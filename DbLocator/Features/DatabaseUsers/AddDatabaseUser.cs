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

        if (command.CreateUser)
        {
            var databaseServers = await dbContext
                .Set<DatabaseEntity>()
                .Where(d => command.DatabaseIds.Contains(d.DatabaseId))
                .Select(d => d.DatabaseServer)
                .Distinct()
                .ToListAsync();

            var processedServers = new HashSet<int>();

            foreach (var databaseServer in databaseServers)
            {
                if (processedServers.Contains(databaseServer.DatabaseServerId))
                    continue;

                await Sql.ExecuteSqlCommandAsync(
                    dbContext,
                    $"create login [{command.UserName}] with password = '{command.UserPassword}'",
                    databaseServer.IsLinkedServer,
                    databaseServer.DatabaseServerHostName
                );

                processedServers.Add(databaseServer.DatabaseServerId);
            }
        }

        var databaseUserId = databaseUser.DatabaseUserId;

        var databaseUserDatabases = command
            .DatabaseIds.Select(databaseId => new DatabaseUserDatabaseEntity
            {
                DatabaseUserId = databaseUserId,
                DatabaseId = databaseId
            })
            .ToList();

        await dbContext.Set<DatabaseUserDatabaseEntity>().AddRangeAsync(databaseUserDatabases);
        await dbContext.SaveChangesAsync();

        // only if create user is specificed, do we ran any sql commands
        if (command.CreateUser)
        {
            foreach (var databaseId in command.DatabaseIds)
            {
                await using var scopedDbContext = await dbContextFactory.CreateDbContextAsync();

                await CreateDatabaseUser(scopedDbContext, databaseId, command.UserName);
            }
        }

        cache?.Remove("databaseUsers");

        return databaseUserId;
    }

    private static async Task CreateDatabaseUser(
        DbLocatorContext dbContext,
        int databaseId,
        string userName
    )
    {
        var database =
            await dbContext
                .Set<DatabaseEntity>()
                .Include(d => d.DatabaseServer)
                .FirstOrDefaultAsync(d => d.DatabaseId == databaseId)
            ?? throw new KeyNotFoundException($"Database with ID {databaseId} not found.");

        var uName = Sql.SanitizeSqlIdentifier(userName);
        var dbName = Sql.SanitizeSqlIdentifier(database.DatabaseName);

        await Sql.ExecuteSqlCommandAsync(
            dbContext,
            $"use [{dbName}]; create user [{uName}] for login [{uName}]",
            database.DatabaseServer.IsLinkedServer,
            database.DatabaseServer.DatabaseServerHostName
        );
    }
}
