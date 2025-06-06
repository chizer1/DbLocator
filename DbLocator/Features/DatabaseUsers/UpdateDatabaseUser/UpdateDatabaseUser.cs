#nullable enable

using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUsers.UpdateDatabaseUser;

internal record UpdateDatabaseUserCommand(
    int DatabaseUserId,
    int[]? DatabaseIds,
    string? UserName,
    string? UserPassword,
    bool? AffectDatabase
);

internal sealed class UpdateDatabaseUserCommandValidator
    : AbstractValidator<UpdateDatabaseUserCommand>
{
    internal UpdateDatabaseUserCommandValidator()
    {
        RuleFor(x => x.DatabaseUserId)
            .GreaterThan(0)
            .WithMessage("Database User Id must be greater than 0.");

        RuleFor(x => x.UserName)
            .NotEmpty()
            .When(x => x.UserName != null)
            .WithMessage("User name cannot be empty.")
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .When(x => x.UserName != null)
            .WithMessage("User name cannot be empty or whitespace.");

        RuleFor(x => x.UserPassword)
            .NotEmpty()
            .When(x => x.UserPassword != null)
            .WithMessage("Password cannot be empty.")
            .MinimumLength(8)
            .When(x => x.UserPassword != null)
            .WithMessage("Password must be at least 8 characters long.");
    }
}

internal class UpdateDatabaseUserHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    Encryption encryption,
    DbLocatorCache? cache
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly Encryption _encryption = encryption;
    private readonly DbLocatorCache? _cache = cache;

    public async Task Handle(
        UpdateDatabaseUserCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await new UpdateDatabaseUserCommandValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var user =
            await dbContext
                .Set<DatabaseUserEntity>()
                .FirstOrDefaultAsync(
                    u => u.DatabaseUserId == request.DatabaseUserId,
                    cancellationToken
                )
            ?? throw new KeyNotFoundException(
                $"Database user with ID {request.DatabaseUserId} not found"
            );

        var oldUserName = user.UserName;
        var oldPassword = user.UserPassword != null ? _encryption.Decrypt(user.UserPassword) : null;
        var userNameChanged = false;
        var passwordChanged = false;

        if (request.UserName != null && request.UserName != user.UserName)
        {
            // Check for duplicate username
            var existingUser = await dbContext
                .Set<DatabaseUserEntity>()
                .FirstOrDefaultAsync(
                    u => u.UserName == request.UserName && u.DatabaseUserId != user.DatabaseUserId,
                    cancellationToken
                );
            if (existingUser != null)
            {
                throw new InvalidOperationException(
                    $"User with name '{request.UserName}' already exists"
                );
            }

            userNameChanged = true;
            user.UserName = request.UserName;
        }

        if (request.UserPassword != null)
        {
            if (_encryption.Encrypt(request.UserPassword) != user.UserPassword)
            {
                passwordChanged = true;
                user.UserPassword = _encryption.Encrypt(request.UserPassword);
            }
        }

        List<int> newDatabaseIds = new();
        if (request.DatabaseIds != null)
        {
            // Get current associations before change
            var currentDatabaseIds = await dbContext
                .Set<DatabaseUserDatabaseEntity>()
                .Where(d => d.DatabaseUserId == user.DatabaseUserId)
                .Select(d => d.DatabaseId)
                .ToListAsync(cancellationToken);

            // Remove existing database relationships
            var existingDatabases = await dbContext
                .Set<DatabaseUserDatabaseEntity>()
                .Where(d => d.DatabaseUserId == user.DatabaseUserId)
                .ToListAsync(cancellationToken);
            dbContext.Set<DatabaseUserDatabaseEntity>().RemoveRange(existingDatabases);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Add new database relationships and track which are new
            foreach (var databaseId in request.DatabaseIds)
            {
                var database =
                    await dbContext
                        .Set<DatabaseEntity>()
                        .FirstOrDefaultAsync(d => d.DatabaseId == databaseId, cancellationToken)
                    ?? throw new KeyNotFoundException($"Database with ID {databaseId} not found.");

                await dbContext
                    .Set<DatabaseUserDatabaseEntity>()
                    .AddAsync(
                        new DatabaseUserDatabaseEntity
                        {
                            DatabaseId = database.DatabaseId,
                            DatabaseUserId = user.DatabaseUserId
                        },
                        cancellationToken
                    );
                if (!currentDatabaseIds.Contains(databaseId))
                {
                    newDatabaseIds.Add(databaseId);
                }
            }
            // Save changes after adding new associations
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        dbContext.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        // DDL logic for AffectDatabase
        if (request.AffectDatabase == true && (userNameChanged || passwordChanged))
        {
            // Get all databases the user is now associated with
            var userDatabaseIds =
                request.DatabaseIds
                ?? await dbContext
                    .Set<DatabaseUserDatabaseEntity>()
                    .Where(d => d.DatabaseUserId == user.DatabaseUserId)
                    .Select(d => d.DatabaseId)
                    .ToArrayAsync(cancellationToken);

            var updatedDatabases = await dbContext
                .Set<DatabaseEntity>()
                .Include(d => d.DatabaseServer)
                .Where(d => userDatabaseIds.Contains(d.DatabaseId))
                .ToListAsync(cancellationToken);

            foreach (var database in updatedDatabases)
            {
                var commands = new List<string>();
                var sanitizedDbName = Sql.SanitizeSqlIdentifier(database.DatabaseName);
                var sanitizedOldUserName =
                    oldUserName != null ? Sql.SanitizeSqlIdentifier(oldUserName) : null;
                var sanitizedNewUserName =
                    request.UserName != null ? Sql.SanitizeSqlIdentifier(request.UserName) : null;

                // If this is a new association, ensure login and user exist
                if (newDatabaseIds.Contains(database.DatabaseId) && sanitizedNewUserName != null)
                {
                    // Use the provided password or a default one if not changing password
                    var sanitizedPassword = (
                        request.UserPassword ?? oldPassword ?? "TempP@ssw0rd!"
                    ).Replace("'", "''");

                    // Ensure login exists at the server level
                    commands.Add(
                        "IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = '"
                            + sanitizedNewUserName
                            + "') "
                            + "BEGIN CREATE LOGIN ["
                            + sanitizedNewUserName
                            + "] WITH PASSWORD = '"
                            + sanitizedPassword
                            + "' END"
                    );

                    // Ensure user exists in the database
                    commands.Add(
                        "USE ["
                            + sanitizedDbName
                            + "]; IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = '"
                            + sanitizedNewUserName
                            + "') "
                            + "BEGIN CREATE USER ["
                            + sanitizedNewUserName
                            + "] FOR LOGIN ["
                            + sanitizedNewUserName
                            + "] END"
                    );
                }
                // If renaming, alter user in all associated databases
                if (userNameChanged && sanitizedOldUserName != null && sanitizedNewUserName != null)
                {
                    commands.Add(
                        "use ["
                            + sanitizedDbName
                            + "]; alter user ["
                            + sanitizedOldUserName
                            + "] with name = ["
                            + sanitizedNewUserName
                            + "]"
                    );
                    commands.Add(
                        "alter login ["
                            + sanitizedOldUserName
                            + "] with name = ["
                            + sanitizedNewUserName
                            + "]"
                    );
                }
                if (passwordChanged && sanitizedNewUserName != null && request.UserPassword != null)
                {
                    var sanitizedPassword = request.UserPassword.Replace("'", "''");
                    commands.Add(
                        "alter login ["
                            + sanitizedNewUserName
                            + "] with password = '"
                            + sanitizedPassword
                            + "'"
                    );
                }
                foreach (var cmd in commands)
                {
                    await Sql.ExecuteSqlCommandAsync(
                        dbContext,
                        cmd,
                        database.DatabaseServer.IsLinkedServer,
                        database.DatabaseServer.IsLinkedServer
                            ? database.DatabaseServer.DatabaseServerHostName
                            : null
                    );
                }
            }
        }

        if (_cache != null)
        {
            await _cache.Remove("databaseUsers");
            await _cache.Remove($"database-user-id-{request.DatabaseUserId}");
            await _cache.Remove("connections");
            await _cache.TryClearConnectionStringFromCache(user.DatabaseUserId);
        }
    }
}
