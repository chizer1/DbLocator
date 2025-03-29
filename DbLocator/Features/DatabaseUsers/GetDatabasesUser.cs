using System.Text.Json;
using DbLocator.Db;
using DbLocator.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Features.DatabaseUsers;

internal class GetDatabaseUsersQuery { }

internal sealed class GetDatabaseUsersQueryValidator : AbstractValidator<GetDatabaseUsersQuery>
{
    internal GetDatabaseUsersQueryValidator() { }
}

internal class GetDatabaseUsers(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    IDistributedCache cache
)
{
    public async Task<List<DatabaseUser>> Handle(GetDatabaseUsersQuery query)
    {
        await new GetDatabaseUsersQueryValidator().ValidateAndThrowAsync(query);

        var cacheKey = "databaseUsers";
        var cachedData = await cache.GetStringAsync(cacheKey);

        if (!string.IsNullOrEmpty(cachedData))
            return JsonSerializer.Deserialize<List<DatabaseUser>>(cachedData);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseUserEntities = await dbContext
            .Set<DatabaseUserEntity>()
            .Include(d => d.UserRoles)
            .ToListAsync();

        var databaseUsers = databaseUserEntities
            .Select(d => new DatabaseUser(
                d.DatabaseUserId,
                d.UserName,
                d.DatabaseId,
                d.UserRoles.Select(ur => (DatabaseRole)ur.DatabaseRoleId).ToList()
            ))
            .ToList();

        var serializedData = JsonSerializer.Serialize(databaseUsers);
        await cache.SetStringAsync(cacheKey, serializedData);

        return databaseUsers;
    }
}
