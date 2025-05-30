#nullable enable

using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUsers.DeleteDatabaseUser;

internal record DeleteDatabaseUserCommand(int DatabaseUserId, bool DeleteDatabaseUser);

internal sealed class DeleteDatabaseUserCommandValidator
    : AbstractValidator<DeleteDatabaseUserCommand>
{
    internal DeleteDatabaseUserCommandValidator()
    {
        RuleFor(x => x.DatabaseUserId)
            .GreaterThan(0)
            .WithMessage("Database User Id must be greater than 0.");
    }
}

internal class DeleteDatabaseUserHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task Handle(
        DeleteDatabaseUserCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await new DeleteDatabaseUserCommandValidator().ValidateAndThrowAsync(
            request,
            cancellationToken
        );

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var databaseUserEntity =
            await dbContext
                .Set<DatabaseUserEntity>()
                .Include(du => du.Databases)
                .FirstOrDefaultAsync(
                    d => d.DatabaseUserId == request.DatabaseUserId,
                    cancellationToken
                ) ?? throw new KeyNotFoundException("Database user not found.");

        var databaseUserDatabases = databaseUserEntity.Databases.ToList();

        if (request.DeleteDatabaseUser)
        {
            var databases = await dbContext
                .Set<DatabaseUserDatabaseEntity>()
                .Include(dud => dud.Database)
                .ThenInclude(db => db.DatabaseServer)
                .Where(dud => dud.DatabaseUserId == databaseUserEntity.DatabaseUserId)
                .ToListAsync(cancellationToken);

            foreach (var database in databases)
            {
                await using var scopedDbContext = await _dbContextFactory.CreateDbContextAsync();

                await DropDatabaseUserAsync(scopedDbContext, databaseUserEntity, database.Database);
            }
        }

        dbContext.Set<DatabaseUserDatabaseEntity>().RemoveRange(databaseUserDatabases);
        await dbContext.SaveChangesAsync(cancellationToken);

        dbContext.Set<DatabaseUserEntity>().Remove(databaseUserEntity);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (_cache != null)
            await _cache.Remove("databaseUsers");
    }

    private static async Task DropDatabaseUserAsync(
        DbLocatorContext dbContext,
        DatabaseUserEntity databaseUser,
        DatabaseEntity database
    )
    {
        var uName = Sql.SanitizeSqlIdentifier(databaseUser.UserName);
        var dbName = Sql.SanitizeSqlIdentifier(database.DatabaseName);

        await Sql.ExecuteSqlCommandAsync(
            dbContext,
            $"use [{dbName}]; drop user [{uName}]",
            database.DatabaseServer.IsLinkedServer,
            database.DatabaseServer.DatabaseServerHostName
                ?? database.DatabaseServer.DatabaseServerName
        );

        await Sql.ExecuteSqlCommandAsync(
            dbContext,
            $"drop login [{uName}]",
            database.DatabaseServer.IsLinkedServer,
            database.DatabaseServer.DatabaseServerHostName
                ?? database.DatabaseServer.DatabaseServerName
        );
    }
}
