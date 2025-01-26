using FluentValidation;

namespace DbLocator.Features.Tenants.DeleteTenant;

internal record DeleteTenantCommand(int TenantId);

internal sealed class DeleteTenantCommandValidator : AbstractValidator<DeleteTenantCommand>
{
    public DeleteTenantCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("Id is required.");
    }
}

internal class DeleteTenant(ITenantRepository tenantRepository)
{
    private readonly ITenantRepository _tenantRepository = tenantRepository;

    public async Task Handle(DeleteTenantCommand command)
    {
        var validator = new DeleteTenantCommandValidator();
        await validator.ValidateAndThrowAsync(command);

        await _tenantRepository.DeleteTenant(command.TenantId);
    }
}
