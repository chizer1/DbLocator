using DbLocator.Domain;
using FluentValidation;

namespace DbLocator.Features.DatabaseTypes.GetDatabaseTypes;

internal class GetDatabaseTypesQuery { }

internal sealed class GetDatabaseTypesQueryValidator : AbstractValidator<GetDatabaseTypesQuery>
{
    public GetDatabaseTypesQueryValidator() { }
}

internal class GetDatabaseTypes(IDatabaseTypeRepository databaseTypeRepository)
{
    private readonly IDatabaseTypeRepository _databaseTypeRepository = databaseTypeRepository;

    public async Task<List<DatabaseType>> Handle(GetDatabaseTypesQuery query)
    {
        var validator = new GetDatabaseTypesQueryValidator();
        await validator.ValidateAndThrowAsync(query);

        return await _databaseTypeRepository.GetDatabaseTypes();
    }
}
