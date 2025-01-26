using DbLocator.Domain;
using FluentValidation;

namespace DbLocator.Features.Tenants.AddTenant;

internal record AddTenantCommand(string TenantName, string TenantCode, Status TenantStatus);

internal sealed class AddTenantCommandValidator : AbstractValidator<AddTenantCommand>
{
    public AddTenantCommandValidator()
    {
        RuleFor(x => x.TenantName).NotEmpty().WithMessage("Tenant Name is required.");
        RuleFor(x => x.TenantCode).NotEmpty().WithMessage("Tenant Code is required.");
        RuleFor(x => x.TenantStatus).NotEmpty().WithMessage("Tenant Status is required.");
    }
}

internal class AddTenant(ITenantRepository tenantRepository)
{
    private readonly ITenantRepository _tenantRepository = tenantRepository;

    public async Task<int> Handle(AddTenantCommand command)
    {
        var validator = new AddTenantCommandValidator();
        await validator.ValidateAndThrowAsync(command);

        return await _tenantRepository.AddTenant(
            command.TenantName,
            command.TenantCode,
            command.TenantStatus
        );
    }
}
