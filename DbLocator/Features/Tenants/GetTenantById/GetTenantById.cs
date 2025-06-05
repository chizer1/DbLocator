#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Tenants.GetTenantById;

internal record GetTenantByIdQuery(int TenantId);

internal sealed class GetTenantByIdQueryValidator : AbstractValidator<GetTenantByIdQuery>
{
    internal GetTenantByIdQueryValidator()
    {
        RuleFor(x => x.TenantId).GreaterThan(0).WithMessage("Tenant Id must be greater than 0.");
    }
}

internal class GetTenantByIdHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task<Tenant> Handle(
        GetTenantByIdQuery request,
        CancellationToken cancellationToken = default
    )
    {
        await new GetTenantByIdQueryValidator().ValidateAndThrowAsync(request, cancellationToken);

        var cacheKey = $"tenant-id-{request.TenantId}";
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
                .FirstOrDefaultAsync(t => t.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant with ID {request.TenantId} not found.");

        var tenant = new Tenant(
            tenantEntity.TenantId,
            tenantEntity.TenantName,
            tenantEntity.TenantCode,
            (Status)tenantEntity.TenantStatusId
        );

        if (_cache != null)
        {
            await _cache.CacheData(cacheKey, tenant);
            var tenants = await _cache.GetCachedData<List<Tenant>>("tenants") ?? [];
            var existingTenant = tenants.FirstOrDefault(t => t.Id == tenant.Id);
            tenants.Add(tenant);
            await _cache.CacheData("tenants", tenants);
        }

        return tenant;
    }
}
