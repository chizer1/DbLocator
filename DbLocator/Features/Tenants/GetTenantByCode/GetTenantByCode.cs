#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Tenants.GetTenantByCode;

internal record GetTenantByCodeQuery(string TenantCode);

internal sealed class GetTenantByCodeQueryValidator : AbstractValidator<GetTenantByCodeQuery>
{
    internal GetTenantByCodeQueryValidator()
    {
        RuleFor(x => x.TenantCode).NotEmpty().WithMessage("Tenant Code is required.");
    }
}

internal class GetTenantByCodeHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<Tenant> Handle(
        GetTenantByCodeQuery request,
        CancellationToken cancellationToken = default
    )
    {
        await new GetTenantByCodeQueryValidator().ValidateAndThrowAsync(request, cancellationToken);

        var cacheKey = $"tenant-code-{request.TenantCode}";
        if (_cache != null)
        {
            var cachedData = await _cache.GetCachedData<Tenant>(cacheKey);
            if (cachedData != null)
                return cachedData;
        }

        await using var dbContext = _dbContextFactory.CreateDbContext();
        var tenantEntity =
            await dbContext
                .Set<TenantEntity>()
                .FirstOrDefaultAsync(t => t.TenantCode == request.TenantCode, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant with code {request.TenantCode} not found.");

        var tenant = new Tenant(
            tenantEntity.TenantId,
            tenantEntity.TenantName,
            tenantEntity.TenantCode,
            (Status)tenantEntity.TenantStatusId
        );

        if (_cache != null)
        {
            await _cache.CacheData(cacheKey, tenant);
            await _cache.CacheData($"tenant-id-{tenant.Id}", tenant);
            var tenants = await _cache.GetCachedData<List<Tenant>>("tenants") ?? [];
            var existingTenant = tenants.FirstOrDefault(t => t.Id == tenant.Id);
            if (existingTenant != null)
            {
                tenants.Remove(existingTenant);
            }
            tenants.Add(tenant);
            await _cache.CacheData("tenants", tenants);
        }

        return tenant;
    }
}
