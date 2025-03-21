using DbLocator.Db;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace DbLocator.Features.Tenants;

internal record DeleteTenantCommand(int TenantId);

internal sealed class DeleteTenantCommandValidator : AbstractValidator<DeleteTenantCommand>
{
    internal DeleteTenantCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Tenant Id is required.");
    }
}

internal class DeleteTenant(
    IDbContextFactory<DbLocatorContext> dbContextFactory,
    IDistributedCache cache
)
{
    internal async Task Handle(DeleteTenantCommand command)
    {
        await new DeleteTenantCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var tenant =
            await dbContext
                .Set<TenantEntity>()
                .FirstOrDefaultAsync(c => c.TenantId == command.TenantId)
            ?? throw new KeyNotFoundException($"Tenant '{command.TenantId}' not found.");

        if (await dbContext.Set<ConnectionEntity>().AnyAsync(c => c.TenantId == command.TenantId))
            throw new InvalidOperationException(
                "Cannot delete tenant because there are connections associated with it, please delete the connections first."
            );

        dbContext.Set<TenantEntity>().Remove(tenant);
        await dbContext.SaveChangesAsync();

        cache?.Remove("tenants");
    }
}
