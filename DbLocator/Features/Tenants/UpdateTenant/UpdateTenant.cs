#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Tenants.UpdateTenant;

internal record UpdateTenantCommand(
    int TenantId,
    string TenantName,
    string TenantCode,
    Status? TenantStatus
);

internal sealed class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    internal UpdateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant Id is required.");

        RuleFor(x => x.TenantName)
            .MaximumLength(50)
            .WithMessage("Tenant Name cannot be more than 50 characters.");

        RuleFor(x => x.TenantCode)
            .MaximumLength(10)
            .WithMessage("Tenant Code cannot be more than 10 characters.");

        RuleFor(x => x.TenantStatus).IsInEnum().WithMessage("Tenant Status is invalid.");
    }
}

internal class UpdateTenantHandler(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    DbLocatorCache? cache = null
)
{
    private readonly IDbContextFactory<DbLocatorContext> _dbContextFactory = dbContextFactory;
    private readonly DbLocatorCache? _cache = cache;

    public async Task Handle(
        UpdateTenantCommand command,
        CancellationToken cancellationToken = default
    )
    {
        await new UpdateTenantCommandValidator().ValidateAndThrowAsync(command, cancellationToken);

        await using var dbContext = _dbContextFactory.CreateDbContext();

        var tenant =
            await dbContext
                .Set<TenantEntity>()
                .FirstOrDefaultAsync(c => c.TenantId == command.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant '{command.TenantId}' not found.");

        if (!string.IsNullOrEmpty(command.TenantName))
            tenant.TenantName = command.TenantName;

        if (!string.IsNullOrEmpty(command.TenantCode))
            tenant.TenantCode = command.TenantCode;

        if (command.TenantStatus.HasValue)
            tenant.TenantStatusId = (byte)command.TenantStatus.Value;

        dbContext.Set<TenantEntity>().Update(tenant);
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
