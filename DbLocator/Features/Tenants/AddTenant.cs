using DbLocator.Db;
using DbLocator.Domain;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace DbLocator.Features.Tenants;

internal record AddTenantCommand(string TenantName, string TenantCode, Status TenantStatus);

internal sealed class AddTenantCommandValidator : AbstractValidator<AddTenantCommand>
{
    internal AddTenantCommandValidator()
    {
        RuleFor(x => x.TenantName)
            .NotEmpty()
            .WithMessage("Tenant Name is required.")
            .MaximumLength(50)
            .WithMessage("Tenant Name cannot be more than 50 characters.");

        RuleFor(x => x.TenantCode)
            .MaximumLength(10)
            .WithMessage("Tenant Code cannot be more than 10 characters.");

        RuleFor(x => x.TenantStatus).IsInEnum().WithMessage("Tenant Status is invalid.");
    }
}

internal class AddTenant(IDbContextFactory<DbLocatorContext> dbContextFactory)
{
    internal async Task<int> Handle(AddTenantCommand command)
    {
        await new AddTenantCommandValidator().ValidateAndThrowAsync(command);

        await using var dbContext = dbContextFactory.CreateDbContext();

        if (await dbContext.Set<TenantEntity>().AnyAsync(c => c.TenantName == command.TenantName))
            throw new InvalidOperationException($"Tenant '{command.TenantName}' already exists.");

        var tenant = new TenantEntity
        {
            TenantName = command.TenantName,
            TenantCode = command.TenantCode,
            TenantStatusId = (byte)command.TenantStatus,
        };

        await dbContext.Set<TenantEntity>().AddAsync(tenant);
        await dbContext.SaveChangesAsync();

        return tenant.TenantId;
    }
}
