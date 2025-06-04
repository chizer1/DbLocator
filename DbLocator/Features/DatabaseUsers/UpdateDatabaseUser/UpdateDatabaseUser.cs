#nullable enable

using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
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

        if (request.UserName != null)
        {
            if (
                await dbContext
                    .Set<DatabaseUserEntity>()
                    .AnyAsync(
                        u =>
                            u.UserName == request.UserName
                            && u.DatabaseUserId != request.DatabaseUserId,
                        cancellationToken
                    )
            )
                throw new InvalidOperationException(
                    $"Database user with name \"{request.UserName}\" already exists"
                );

            user.UserName = request.UserName;
        }

        if (request.UserPassword != null)
        {
            if (request.UserPassword.Length < 8)
            {
                throw new InvalidOperationException("Password must be at least 8 characters long");
            }
            user.UserPassword = _encryption.Encrypt(request.UserPassword);
        }

        if (request.DatabaseIds != null)
        {
            // Remove existing database relationships
            var existingDatabases = await dbContext
                .Set<DatabaseUserDatabaseEntity>()
                .Where(d => d.DatabaseUserId == user.DatabaseUserId)
                .ToListAsync(cancellationToken);
            dbContext.Set<DatabaseUserDatabaseEntity>().RemoveRange(existingDatabases);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Add new database relationships
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
            }
        }

        dbContext.Update(user);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (_cache != null)
        {
            await _cache.Remove("databaseUsers");
            await _cache.Remove($"database-user-id-{request.DatabaseUserId}");
            await _cache.Remove("connections");
            await _cache.TryClearConnectionStringFromCache(user.DatabaseUserId);
        }
    }
}
