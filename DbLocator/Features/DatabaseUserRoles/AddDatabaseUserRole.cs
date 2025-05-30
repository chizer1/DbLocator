using DbLocator.Db;
using DbLocator.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUserRoles;

internal record AddDatabaseUserRoleCommand(
    int DatabaseUserId,
    DatabaseRole UserRole,
    bool AffectDatabase
);

internal sealed class AddDatabaseUserRoleCommandValidator
    : AbstractValidator<AddDatabaseUserRoleCommand>
{
    internal AddDatabaseUserRoleCommandValidator()
    {
        RuleFor(x => x.DatabaseUserId).NotEmpty().WithMessage("Database User Id is required");
    }
}

internal class AddDatabaseUserRole(IDbContextFactory<DbLocatorContext> dbContextFactory)
{
    internal async Task<int> Handle(AddDatabaseUserRoleCommand command)
    {
        await new AddDatabaseUserRoleCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var user =
            await dbContext
                .Set<DatabaseUserEntity>()
                .Include(u => u.UserRoles)
                .Where(u => u.DatabaseUserId == command.DatabaseUserId)
                .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException(
                $"Database User Id '{command.DatabaseUserId}' not found."
            );

        var existingRole = user.UserRoles.FirstOrDefault(ur =>
            ur.DatabaseRoleId == (int)command.UserRole
        );

        if (existingRole != null)
            throw new InvalidOperationException(
                $"User '{user.UserName}' already has role '{command.UserRole}'."
            );

        var databaseUserRole = new DatabaseUserRoleEntity()
        {
            DatabaseUserId = command.DatabaseUserId,
            DatabaseRoleId = (int)command.UserRole
        };

        await dbContext.Set<DatabaseUserRoleEntity>().AddAsync(databaseUserRole);
        await dbContext.SaveChangesAsync();

        if (command.AffectDatabase)
            await CreateDatabaseUserRole(dbContext, user, command);

        return databaseUserRole.DatabaseUserRoleId;
    }

    private static async Task CreateDatabaseUserRole(
        DbLocatorContext dbContext,
        DatabaseUserEntity user,
        AddDatabaseUserRoleCommand command
    )
    {
        var databases = await dbContext
            .Set<DatabaseUserDatabaseEntity>()
            .Include(dud => dud.Database)
            .Include(dud => dud.Database.DatabaseServer)
            .Where(dud => dud.DatabaseUserId == user.DatabaseUserId)
            .Select(dud => dud.Database)
            .ToListAsync();

        foreach (var database in databases)
        {
            var dbName = Sql.SanitizeSqlIdentifier(database.DatabaseName);
            var userName = Sql.SanitizeSqlIdentifier(user.UserName);
            var roleName = Sql.SanitizeSqlIdentifier($"db_{command.UserRole.ToString().ToLower()}");

            await Sql.ExecuteSqlCommandAsync(
                dbContext,
                $"use [{dbName}]; exec sp_addrolemember '{roleName}', '{userName}';",
                database.DatabaseServer.IsLinkedServer,
                database.DatabaseServer.DatabaseServerHostName
            );
        }
    }
}
