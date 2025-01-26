using System.Data.SqlClient;
using DbLocator.Domain;
using DbLocator.Features.Tenants;
using FluentValidation;

namespace DbLocator.Features.Connections.GetConnection;

internal record GetConnectionQuery(int TenantId, int DatabaseTypeId);

internal sealed class GetConnectionQueryValidator : AbstractValidator<GetConnectionQuery>
{
    public GetConnectionQueryValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty().WithMessage("TenantId is required");
        RuleFor(x => x.DatabaseTypeId).NotEmpty().WithMessage("DatabaseTypeId is required");
    }
}

internal class GetConnection(
    IConnectionRepository connectionRepository,
    ITenantRepository tenantRepository
)
{
    public async Task<SqlConnection> Handle(GetConnectionQuery query)
    {
        await new GetConnectionQueryValidator().ValidateAndThrowAsync(query);

        var tenant = await tenantRepository.GetTenant(query.TenantId);
        if (tenant.Status == Status.Inactive)
            throw new Exception("Cannot connect to database since this tenant is inactive.");

        return await connectionRepository.GetSqlConnection(query.TenantId, query.DatabaseTypeId);
    }
}
