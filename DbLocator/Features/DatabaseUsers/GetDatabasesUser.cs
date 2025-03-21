using DbLocator.Db;
using DbLocator.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseUsers;

internal class GetDatabaseUsersQuery { }

internal sealed class GetDatabaseUsersQueryValidator : AbstractValidator<GetDatabaseUsersQuery>
{
    internal GetDatabaseUsersQueryValidator() { }
}

internal class GetDatabaseUsers(IDbContextFactory<DbLocatorContext> dbContextFactory)
{
    public async Task<List<DatabaseUser>> Handle(GetDatabaseUsersQuery query)
    {
        await new GetDatabaseUsersQueryValidator().ValidateAndThrowAsync(query);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseUserEntities = await dbContext
            .Set<DatabaseUserEntity>()
            .Include(d => d.UserRoles)
            .ToListAsync();

        return
        [
            .. databaseUserEntities.Select(d => new DatabaseUser(
                d.DatabaseUserId,
                d.UserName,
                d.DatabaseId,
                [.. d.UserRoles.Select(ur => (DatabaseRole)ur.DatabaseRoleId)]
            ))
        ];
    }
}
