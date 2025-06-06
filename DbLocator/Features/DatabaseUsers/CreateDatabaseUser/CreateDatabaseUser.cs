#nullable enable

using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUsers.CreateDatabaseUser;

internal record CreateDatabaseUserCommand(
    string UserName,
    string UserPassword,
    int[] DatabaseIds,
    bool AffectDatabase
);

internal sealed class CreateDatabaseUserCommandValidator
    : AbstractValidator<CreateDatabaseUserCommand>
{
    internal CreateDatabaseUserCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("User name is required.");

        RuleFor(x => x.UserPassword)
            .NotEmpty()
            .WithMessage("User password is required.")
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.");

        RuleFor(x => x.DatabaseIds).NotEmpty().WithMessage("Database ids are required.");
    }
}

internal class CreateDatabaseUserHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    Encryption encryption,
    DbLocatorCache? cache
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly Encryption _encryption = encryption;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<int> Handle(
        CreateDatabaseUserCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await new CreateDatabaseUserCommandValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var nonExistentDatabaseIds = request
            .DatabaseIds.Where(id =>
                !dbContext.Set<DatabaseEntity>().Any(ds => ds.DatabaseId == id)
            )
            .ToList();

        if (nonExistentDatabaseIds.Count != 0)
        {
            throw new KeyNotFoundException(
                $"Database(s) not found with ID(s): {string.Join(", ", nonExistentDatabaseIds)}"
            );
        }

        if (
            await dbContext
                .Set<DatabaseUserEntity>()
                .AnyAsync(u => u.UserName == request.UserName, cancellationToken)
        )
        {
            throw new InvalidOperationException(
                $"Database user with name \"{request.UserName}\" already exists"
            );
        }

        var databaseUser = new DatabaseUserEntity()
        {
            UserName = request.UserName,
            UserPassword = _encryption.Encrypt(request.UserPassword)
        };

        await dbContext.Set<DatabaseUserEntity>().AddAsync(databaseUser, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var databaseUserId = databaseUser.DatabaseUserId;

        var databaseUserDatabases = request
            .DatabaseIds.Select(databaseId => new DatabaseUserDatabaseEntity
            {
                DatabaseUserId = databaseUserId,
                DatabaseId = databaseId
            })
            .ToList();

        await dbContext
            .Set<DatabaseUserDatabaseEntity>()
            .AddRangeAsync(databaseUserDatabases, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (request.AffectDatabase)
        {
            var databaseServers = await dbContext
                .Set<DatabaseEntity>()
                .Where(d => request.DatabaseIds.Contains(d.DatabaseId))
                .Select(d => d.DatabaseServer)
                .Distinct()
                .ToListAsync(cancellationToken);

            var processedServers = new HashSet<int>();
            foreach (var databaseServer in databaseServers)
            {
                if (!processedServers.Contains(databaseServer.DatabaseServerId))
                {
                    await Sql.ExecuteSqlCommandAsync(
                        dbContext,
                        $"create login [{request.UserName}] with password = '{request.UserPassword}'",
                        databaseServer.IsLinkedServer,
                        databaseServer.DatabaseServerHostName
                    );
                    processedServers.Add(databaseServer.DatabaseServerId);
                }
            }

            foreach (var databaseId in request.DatabaseIds)
            {
                await using var scopedDbContext = await _dbContextFactory.CreateDbContextAsync();

                await CreateDatabaseUser(
                    scopedDbContext,
                    databaseId,
                    request.UserName,
                    cancellationToken
                );
            }
        }

        if (_cache != null)
            await _cache.Remove("databaseUsers");

        return databaseUserId;
    }

    private static async Task CreateDatabaseUser(
        DbLocatorContext dbContext,
        int databaseId,
        string userName,
        CancellationToken cancellationToken
    )
    {
        var database =
            await dbContext
                .Set<DatabaseEntity>()
                .Include(d => d.DatabaseServer)
                .FirstOrDefaultAsync(d => d.DatabaseId == databaseId, cancellationToken)
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
