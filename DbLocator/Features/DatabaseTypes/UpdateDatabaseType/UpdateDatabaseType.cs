using FluentValidation;

namespace DbLocator.Features.DatabaseTypes.UpdateDatabaseType;

internal record UpdateDatabaseTypeCommand(int DatabaseTypeId, string DatabaseTypeName);

internal sealed class UpdateDatabaseTypeCommandValidator
    : AbstractValidator<UpdateDatabaseTypeCommand>
{
    public UpdateDatabaseTypeCommandValidator()
    {
        RuleFor(x => x.DatabaseTypeId).NotNull().WithMessage("DatabaseTypeId cannot be null");
        RuleFor(x => x.DatabaseTypeName).NotEmpty().WithMessage("DatabaseType Name is required.");
    }
}

internal class UpdateDatabaseType(IDatabaseTypeRepository databaseTypeRepository)
{
    private readonly IDatabaseTypeRepository _databaseTypeRepository = databaseTypeRepository;

    public async Task Handle(UpdateDatabaseTypeCommand command)
    {
        await new UpdateDatabaseTypeCommandValidator().ValidateAndThrowAsync(command);

        await _databaseTypeRepository.UpdateDatabaseType(
            command.DatabaseTypeId,
            command.DatabaseTypeName
        );
    }
}
