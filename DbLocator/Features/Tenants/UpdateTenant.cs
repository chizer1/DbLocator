using DbLocator.Db;
using DbLocator.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Tenants;

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

internal class UpdateTenant(IDbContextFactory<DbLocatorContext> dbContextFactory)
{
    internal async Task Handle(UpdateTenantCommand command)
    {
        await new UpdateTenantCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        var tenant =
            await dbContext
                .Set<TenantEntity>()
                .FirstOrDefaultAsync(c => c.TenantId == command.TenantId)
            ?? throw new KeyNotFoundException($"Tenant '{command.TenantId}' not found.");

        if (!string.IsNullOrEmpty(command.TenantName))
            tenant.TenantName = command.TenantName;

        if (!string.IsNullOrEmpty(command.TenantCode))
            tenant.TenantCode = command.TenantCode;

        if (command.TenantStatus.HasValue)
            tenant.TenantStatusId = (byte)command.TenantStatus.Value;

        dbContext.Set<TenantEntity>().Update(tenant);
        await dbContext.SaveChangesAsync();
    }
}
