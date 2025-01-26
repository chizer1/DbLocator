using DbLocator.Domain;
using FluentValidation;

namespace DbLocator.Features.Tenants.UpdateTenant;

internal record UpdateTenantCommand(
    int TenantId,
    string TenantName,
    string TenantCode,
    Status TenantStatus
);

internal sealed class UpdateTenantCommandValidator : AbstractValidator<UpdateTenantCommand>
{
    public UpdateTenantCommandValidator()
    {
        RuleFor(x => x.TenantId).NotNull().WithMessage("Id cannot be null");
        RuleFor(x => x.TenantName).NotEmpty().WithMessage("Tenant Name is required.");
        RuleFor(x => x.TenantCode).NotEmpty().WithMessage("Tenant Code is required.");
        RuleFor(x => x.TenantStatus).NotEmpty().WithMessage("Tenant Status is required.");
    }
}

internal class UpdateTenant(ITenantRepository TenantRepository)
{
    public async Task Handle(UpdateTenantCommand command)
    {
        await new UpdateTenantCommandValidator().ValidateAndThrowAsync(command);

        await TenantRepository.UpdateTenant(
            command.TenantId,
            command.TenantName,
            command.TenantCode,
            command.TenantStatus
        );
    }
}
