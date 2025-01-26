using DbLocator.Domain;
using FluentValidation;

namespace DbLocator.Features.Tenants.GetTenants;

internal class GetTenantsQuery { }

internal sealed class GetTenantsQueryValidator : AbstractValidator<GetTenantsQuery>
{
    public GetTenantsQueryValidator() { }
}

internal class GetTenants(ITenantRepository tenantRepository)
{
    private readonly ITenantRepository _tenantRepository = tenantRepository;

    public async Task<List<Tenant>> Handle(GetTenantsQuery query)
    {
        var validator = new GetTenantsQueryValidator();
        await validator.ValidateAndThrowAsync(query);

        return await _tenantRepository.GetTenants();
    }
}
