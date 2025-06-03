#nullable enable

using DbLocator.Db;
using DbLocator.Domain;
using DbLocator.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Tenants.UpdateTenant;

internal record UpdateTenantCommand(
    int TenantId,
    string? TenantName = null,
    string? TenantCode = null,
    Status? TenantStatus = null
);

internal sealed class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    internal UpdateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId).GreaterThan(0).WithMessage("Tenant Id must be greater than 0.");

        RuleFor(x => x.TenantName)
            .NotEmpty()
            .When(x => x.TenantName != null)
            .WithMessage("Tenant Name is required.")
            .MaximumLength(50)
            .When(x => x.TenantName != null)
            .WithMessage("Tenant Name cannot be more than 50 characters.");

        RuleFor(x => x.TenantCode)
            .NotEmpty()
            .When(x => x.TenantCode != null)
            .WithMessage("Tenant Code is required.")
            .MaximumLength(10)
            .When(x => x.TenantCode != null)
            .WithMessage("Tenant Code cannot be more than 10 characters.");

        RuleFor(x => x.TenantStatus)
            .IsInEnum()
            .When(x => x.TenantStatus.HasValue)
            .WithMessage("Tenant Status is invalid.");

        RuleFor(x => x)
            .Must(x => x.TenantName != null || x.TenantCode != null || x.TenantStatus.HasValue)
            .WithMessage("At least one field must be provided for update");
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
        UpdateTenantCommand request,
        CancellationToken cancellationToken = default
    )
    {
        await using var dbContext = _dbContextFactory.CreateDbContext();

        var tenant =
            await dbContext
                .Set<TenantEntity>()
                .FirstOrDefaultAsync(t => t.TenantId == request.TenantId, cancellationToken)
            ?? throw new KeyNotFoundException($"Tenant with ID {request.TenantId} not found.");

        // Only validate if we have a valid request
        if (
            request.TenantName != null
            || request.TenantCode != null
            || request.TenantStatus.HasValue
        )
        {
            await new UpdateTenantCommandValidator().ValidateAndThrowAsync(
                request,
                cancellationToken
            );
        }

        if (request.TenantName != null)
        {
            if (
                await dbContext
                    .Set<TenantEntity>()
                    .AnyAsync(
                        t => t.TenantName == request.TenantName && t.TenantId != request.TenantId,
                        cancellationToken
                    )
            )
                throw new InvalidOperationException(
                    $"Tenant with name \"{request.TenantName}\" already exists"
                );

            tenant.TenantName = request.TenantName;
        }

        if (request.TenantCode != null)
        {
            if (
                await dbContext
                    .Set<TenantEntity>()
                    .AnyAsync(
                        t => t.TenantCode == request.TenantCode && t.TenantId != request.TenantId,
                        cancellationToken
                    )
            )
                throw new InvalidOperationException(
                    $"Tenant with code \"{request.TenantCode}\" already exists"
                );

            tenant.TenantCode = request.TenantCode;
        }

        if (request.TenantStatus.HasValue)
        {
            tenant.TenantStatusId = (byte)request.TenantStatus.Value;
        }

        dbContext.Update(tenant);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (_cache != null)
        {
            await _cache.Remove("tenants");
            await _cache.Remove($"tenant-id-{request.TenantId}");
            if (tenant.TenantCode != null)
            {
                await _cache.Remove($"tenant-code-{tenant.TenantCode}");
            }
            await _cache.Remove("connections");
            await _cache.TryClearConnectionStringFromCache(tenantId: request.TenantId);
            if (tenant.TenantCode != null)
            {
                await _cache.TryClearConnectionStringFromCache(tenantCode: tenant.TenantCode);
            }
        }
    }
}

#nullable disable
