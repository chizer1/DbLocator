using FluentValidation;

namespace DbLocator.Features.DatabaseTypes.AddDatabaseType;

internal record AddDatabaseTypeCommand(string DatabaseTypeName);

internal sealed class AddDatabaseTypeCommandValidator : AbstractValidator<AddDatabaseTypeCommand>
{
    public AddDatabaseTypeCommandValidator()
    {
        RuleFor(x => x.DatabaseTypeName).NotEmpty().WithMessage("DatabaseType Name is required.");
    }
}

internal class AddDatabaseType(IDatabaseTypeRepository databaseTypeRepository)
{
    private readonly IDatabaseTypeRepository _databaseTypeRepository = databaseTypeRepository;

    public async Task<int> Handle(AddDatabaseTypeCommand command)
    {
        var validator = new AddDatabaseTypeCommandValidator();
        await validator.ValidateAndThrowAsync(command);

        return await _databaseTypeRepository.AddDatabaseType(command.DatabaseTypeName);
    }
}
