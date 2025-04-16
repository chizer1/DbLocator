using DbLocator.Db;
using DbLocator.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUserRoles;

internal record AddDatabaseUserRoleCommand(
    int DatabaseUserId,
    DatabaseRole UserRole,
    bool UpdateUser
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

        if (command.UpdateUser)
            await CreateDatabaseUserRole(dbContext, user, command);

        return databaseUserRole.DatabaseUserRoleId;
    }

    private static async Task CreateDatabaseUserRole(
        DbLocatorContext dbContext,
        DatabaseUserEntity user,
        AddDatabaseUserRoleCommand command
    )
    {
        var databaseUserDatabase =
            await dbContext
                .Set<DatabaseUserDatabaseEntity>()
                .Include(dud => dud.Database)
                .ThenInclude(d => d.DatabaseServer)
                .FirstOrDefaultAsync(dud => dud.DatabaseUserId == user.DatabaseUserId)
            ?? throw new InvalidOperationException("Database not found.");

        var database = databaseUserDatabase.Database;

        var dbName = Sql.SanitizeSqlIdentifier(database.DatabaseName);
        var userName = Sql.EscapeForDynamicSql(Sql.SanitizeSqlIdentifier(user.UserName));
        var roleName = $"db_{Enum.GetName(command.UserRole).ToLower()}";
        roleName = Sql.EscapeForDynamicSql(Sql.SanitizeSqlIdentifier(roleName));

        var commandText = $"use [{dbName}]; exec sp_addrolemember '{roleName}', '{userName}';";

        if (database.DatabaseServer.IsLinkedServer)
        {
            var linkedServer = Sql.SanitizeSqlIdentifier(
                database.DatabaseServer.DatabaseServerHostName
            );
            commandText = $"exec('{Sql.EscapeForDynamicSql(commandText)}') at [{linkedServer}];";
        }

        using var cmd = dbContext.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = commandText;

        await dbContext.Database.OpenConnectionAsync();
        await cmd.ExecuteNonQueryAsync();
    }
}
