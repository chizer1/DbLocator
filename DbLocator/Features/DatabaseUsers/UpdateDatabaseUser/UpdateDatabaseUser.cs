#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUsers.UpdateDatabaseUser;

internal record UpdateDatabaseUserCommand(
    int DatabaseUserId,
    int[] DatabaseIds,
    string? UserName,
    string? UserPassword,
    bool AffectDatabase = true
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
            .When(x => !string.IsNullOrEmpty(x.UserName))
            .WithMessage("User name cannot be empty.");

        RuleFor(x => x.UserPassword)
            .NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.UserPassword))
            .MinimumLength(8)
            .When(x => !string.IsNullOrEmpty(x.UserPassword))
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

        var databaseUserEntity =
            await dbContext
                .Set<DatabaseUserEntity>()
                .Include(du => du.UserRoles)
                .Include(du => du.Databases)
                .FirstOrDefaultAsync(
                    d => d.DatabaseUserId == request.DatabaseUserId,
                    cancellationToken
                )
            ?? throw new KeyNotFoundException(
                $"Database user with ID {request.DatabaseUserId} not found"
            );

        if (string.IsNullOrWhiteSpace(request.UserName))
            throw new ArgumentNullException(
                nameof(request.UserName),
                "Database user name is required"
            );

        if (
            (
                await dbContext
                    .Set<DatabaseUserEntity>()
                    .Where(u => u.UserName == request.UserName)
                    .AnyAsync(cancellationToken)
            )
            && databaseUserEntity.UserName != request.UserName
        )
        {
            throw new InvalidOperationException(
                $"Database user with name \"{request.UserName}\" already exists"
            );
        }

        var oldDatabaseUserName = databaseUserEntity.UserName;
        var oldDatabasePassword = _encryption.Decrypt(databaseUserEntity.UserPassword);
        var oldDatabaseRoles = databaseUserEntity
            .UserRoles.Select(dr => (DatabaseRole)dr.DatabaseRoleId)
            .ToList();

        if (!string.IsNullOrEmpty(request.UserName))
            databaseUserEntity.UserName = request.UserName;

        if (!string.IsNullOrEmpty(request.UserPassword))
            databaseUserEntity.UserPassword = _encryption.Encrypt(request.UserPassword);

        dbContext.Update(databaseUserEntity);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (!request.AffectDatabase)
            return;

        var currentDatabaseIds = databaseUserEntity.Databases.Select(d => d.DatabaseId).ToList();
        var databasesToAdd = request.DatabaseIds.Except(currentDatabaseIds).ToList();
        var databasesToRemove = currentDatabaseIds.Except(request.DatabaseIds).ToList();

        await dbContext.SaveChangesAsync(cancellationToken);

        var commands = new List<string>();

        var updatedDatabases = await dbContext
            .Set<DatabaseEntity>()
            .Include(d => d.DatabaseServer)
            .Where(d => request.DatabaseIds.Contains(d.DatabaseId))
            .ToListAsync(cancellationToken);

        foreach (var database in updatedDatabases)
        {
            if (oldDatabaseUserName != request.UserName && !string.IsNullOrEmpty(request.UserName))
            {
                var sanitizedOldUserName = Sql.SanitizeSqlIdentifier(
                    oldDatabaseUserName
                        ?? throw new InvalidOperationException("Database user name cannot be null")
                );
                var sanitizedNewUserName = Sql.SanitizeSqlIdentifier(
                    request.UserName
                        ?? throw new InvalidOperationException("Database user name cannot be null")
                );
                var sanitizedDbName = Sql.SanitizeSqlIdentifier(database.DatabaseName);

                commands.Add(
                    $"use [{sanitizedDbName}]; alter user [{sanitizedOldUserName}] with name = [{sanitizedNewUserName}]"
                );
                commands.Add(
                    $"alter login [{sanitizedOldUserName}] with name = [{sanitizedNewUserName}]"
                );
            }

            if (
                oldDatabasePassword != request.UserPassword
                && !string.IsNullOrEmpty(request.UserPassword)
            )
            {
                var sanitizedUserName = Sql.SanitizeSqlIdentifier(
                    request.UserName
                        ?? throw new InvalidOperationException("Database user name cannot be null")
                );
                var sanitizedPassword = request.UserPassword;

                commands.Add(
                    $"alter login [{sanitizedUserName}] with password = '{sanitizedPassword}'"
                );
            }

            for (var i = 0; i < commands.Count; i++)
            {
                await Sql.ExecuteSqlCommandAsync(
                    dbContext,
                    commands[i],
                    database.DatabaseServer.IsLinkedServer,
                    database.DatabaseServer.DatabaseServerHostName
                );
            }
        }

        if (_cache != null)
        {
            await _cache.Remove("databaseUsers");

            var roles = databaseUserEntity
                .UserRoles.Select(ur => (DatabaseRole)ur.DatabaseRoleId)
                .ToArray();

            await _cache.TryClearConnectionStringFromCache(roles: roles);
        }
    }
}
