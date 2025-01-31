using DbLocator.Db;
using DbLocator.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.DatabaseTypes;

internal class GetDatabaseTypesQuery { }

internal sealed class GetDatabaseTypesQueryValidator : AbstractValidator<GetDatabaseTypesQuery>
{
    internal GetDatabaseTypesQueryValidator() { }
}

internal class GetDatabaseTypes(IDbContextFactory<DbLocatorContext> dbContextFactory)
{
    internal async Task<List<DatabaseType>> Handle(GetDatabaseTypesQuery query)
    {
        await new GetDatabaseTypesQueryValidator().ValidateAndThrowAsync(query);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var databaseTypeEntities = await dbContext.Set<DatabaseTypeEntity>().ToListAsync();

        return
        [
            .. databaseTypeEntities.Select(entity => new DatabaseType(
                entity.DatabaseTypeId,
                entity.DatabaseTypeName
            ))
        ];
    }
}
