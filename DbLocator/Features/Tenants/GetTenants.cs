using DbLocator.Db;
using DbLocator.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Tenants;

internal class GetTenantsQuery { }

internal sealed class GetTenantsQueryValidator : AbstractValidator<GetTenantsQuery>
{
    internal GetTenantsQueryValidator() { }
}

internal class GetTenants(IDbContextFactory<DbLocatorContext> dbContextFactory)
{
    internal async Task<List<Tenant>> Handle(GetTenantsQuery query)
    {
        await new GetTenantsQueryValidator().ValidateAndThrowAsync(query);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var tenants = await dbContext.Set<TenantEntity>().ToListAsync();

        return
        [
            .. tenants.Select(entity => new Tenant(
                entity.TenantId,
                entity.TenantName,
                entity.TenantCode,
                (Status)entity.TenantStatusId
            ))
        ];
    }
}
