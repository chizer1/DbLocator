using DbLocator.Domain;
using FluentValidation;

namespace DbLocator.Features.DatabaseServers.GetDatabaseServers;

internal class GetDatabaseServersQuery { }

internal sealed class GetDatabaseServersQueryValidator : AbstractValidator<GetDatabaseServersQuery>
{
    public GetDatabaseServersQueryValidator() { }
}

internal class GetDatabaseServers(IDatabaseServerRepository databaseServerRepository)
{
    private readonly IDatabaseServerRepository _databaseServerRepository = databaseServerRepository;

    public async Task<List<DatabaseServer>> Handle(GetDatabaseServersQuery query)
    {
        var validator = new GetDatabaseServersQueryValidator();
        await validator.ValidateAndThrowAsync(query);

        return await _databaseServerRepository.GetDatabaseServers();
    }
}
