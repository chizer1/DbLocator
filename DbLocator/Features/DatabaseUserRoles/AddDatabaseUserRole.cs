using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
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
    internal async Task Handle(AddDatabaseUserRoleCommand command)
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
        {
            return;
        }

        var databaseUserRole = new DatabaseUserRoleEntity()
        {
            DatabaseUserId = command.DatabaseUserId,
            DatabaseRoleId = (int)command.UserRole
        };

        await dbContext.Set<DatabaseUserRoleEntity>().AddAsync(databaseUserRole);
        await dbContext.SaveChangesAsync();

        if (command.UpdateUser)
        {
            await CreateDatabaseUserRole(dbContext, user, command);
        }
    }

    private static async Task CreateDatabaseUserRole(
        DbLocatorContext dbContext,
        DatabaseUserEntity user,
        AddDatabaseUserRoleCommand command
    )
    {
        var database = await dbContext
            .Set<DatabaseEntity>()
            .Include(d => d.DatabaseServer)
            .FirstOrDefaultAsync(ds => ds.DatabaseId == user.DatabaseId);

        var roleName = Enum.GetName(command.UserRole).ToLower();

        var commands = new List<string>
        {
            $"use {database.DatabaseName}; exec sp_addrolemember 'db_{roleName}', '{user.UserName}'"
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
