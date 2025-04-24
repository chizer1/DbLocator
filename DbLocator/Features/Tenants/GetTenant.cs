using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Tenants;

internal class GetTenantByIdQuery
{
    public int TenantId { get; set; }
}

internal class GetTenantByCodeQuery
{
    public string TenantCode { get; set; }
}

internal sealed class GetTenantByIdQueryValidator : AbstractValidator<GetTenantByIdQuery>
{
    internal GetTenantByIdQueryValidator()
    {
        RuleFor(x => x.TenantId).GreaterThan(0);
    }
}

internal sealed class GetTenantByCodeQueryValidator : AbstractValidator<GetTenantByCodeQuery>
{
    internal GetTenantByCodeQueryValidator()
    {
        RuleFor(x => x.TenantCode).NotEmpty();
    }
}

internal class GetTenant(IDbContextFactory<DbLocatorContext> dbContextFactory, DbLocatorCache cache)
{
    internal async Task<Tenant> Handle(GetTenantByIdQuery query)
    {
        await new GetTenantByIdQueryValidator().ValidateAndThrowAsync(query);

        var cacheKey = $"tenant-id-{query.TenantId}";
        var cachedData = await cache?.GetCachedData<Tenant>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        var tenant = await GetTenantFromDatabaseById(dbContextFactory, query.TenantId);
        await cache?.CacheData(cacheKey, tenant);

        return tenant;
    }

    internal async Task<Tenant> Handle(GetTenantByCodeQuery query)
    {
        await new GetTenantByCodeQueryValidator().ValidateAndThrowAsync(query);

        var cacheKey = $"tenant-code-{query.TenantCode}";
        var cachedData = await cache?.GetCachedData<Tenant>(cacheKey);
        if (cachedData != null)
        {
            return cachedData;
        }

        var tenant = await GetTenantFromDatabaseByCode(dbContextFactory, query.TenantCode);
        await cache?.CacheData(cacheKey, tenant);

        return tenant;
    }

    private static async Task<Tenant> GetTenantFromDatabaseById(
        IDbContextFactory<DbLocatorContext> dbContextFactory,
        int tenantId
    )
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var tenantEntity =
            await dbContext.Set<TenantEntity>().FirstOrDefaultAsync(t => t.TenantId == tenantId)
            ?? throw new KeyNotFoundException($"Tenant with ID {tenantId} not found.");

        return new Tenant(
            tenantEntity.TenantId,
            tenantEntity.TenantName,
            tenantEntity.TenantCode,
            (Status)tenantEntity.TenantStatusId
        );
    }

    private static async Task<Tenant> GetTenantFromDatabaseByCode(
        IDbContextFactory<DbLocatorContext> dbContextFactory,
        string tenantCode
    )
    {
        await using var dbContext = dbContextFactory.CreateDbContext();

        var tenantEntity = await dbContext
            .Set<TenantEntity>()
            .FirstOrDefaultAsync(t => t.TenantCode == tenantCode);

        if (tenantEntity == null)
        {
            throw new KeyNotFoundException($"Tenant with code {tenantCode} not found.");
        }

        return new Tenant(
            tenantEntity.TenantId,
            tenantEntity.TenantName,
            tenantEntity.TenantCode,
            (Status)tenantEntity.TenantStatusId
        );
    }
}
