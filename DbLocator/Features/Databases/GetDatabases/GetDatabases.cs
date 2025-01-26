using DbLocator.Domain;
using FluentValidation;

namespace DbLocator.Features.Databases.GetDatabases;

internal class GetDatabasesQuery { }

internal sealed class GetDatabasesQueryValidator : AbstractValidator<GetDatabasesQuery>
{
    public GetDatabasesQueryValidator() { }
}

internal class GetDatabases(IDatabaseRepository databaseRepository)
{
    private readonly IDatabaseRepository _databaseRepository = databaseRepository;

    public async Task<List<Database>> Handle(GetDatabasesQuery query)
    {
        var validator = new GetDatabasesQueryValidator();
        await validator.ValidateAndThrowAsync(query);

        return await _databaseRepository.GetDatabases();
    }
}
