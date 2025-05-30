#nullable enable

using DbLocator.Db;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Tenants.DeleteTenant;

internal record DeleteTenantCommand(int TenantId);

internal sealed class DeleteTenantCommandValidator : AbstractValidator<DeleteTenantCommand>
{
    internal DeleteTenantCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant Id is required.");
    }
}

internal class DeleteTenantHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task Handle(
        DeleteTenantCommand command,
        CancellationToken cancellationToken = default
    )
    {
        await new DeleteTenantCommandValidator().ValidateAndThrowAsync(command, cancellationToken);

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var tenant =
            await dbContext
                .Set<TenantEntity>()
                .FirstOrDefaultAsync(c => c.TenantId == command.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant with ID {command.TenantId} not found");

        if (
            await dbContext
                .Set<ConnectionEntity>()
                .AnyAsync(c => c.TenantId == command.TenantId, cancellationToken)
        )
            throw new InvalidOperationException(
                $"Cannot delete tenant '{tenant.TenantName}' because it is in use by one or more connections"
            );

        dbContext.Set<TenantEntity>().Remove(tenant);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (_cache != null)
        {
            await _cache.Remove("tenants");
            await _cache.Remove("connections");
            await _cache.TryClearConnectionStringFromCache(tenantCode: tenant.TenantCode);
            await _cache.TryClearConnectionStringFromCache(tenantId: tenant.TenantId);
        }
    }
}

#nullable disable
