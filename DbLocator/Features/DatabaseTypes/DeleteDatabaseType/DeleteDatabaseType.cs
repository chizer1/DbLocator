using FluentValidation;

namespace DbLocator.Features.DatabaseTypes.DeleteDatabaseType;

internal record DeleteDatabaseTypeCommand(int DatabaseTypeId);

internal sealed class DeleteDatabaseTypeCommandValidator
    : AbstractValidator<DeleteDatabaseTypeCommand>
{
    public DeleteDatabaseTypeCommandValidator()
    {
        RuleFor(x => x.DatabaseTypeId).NotEmpty().WithMessage("DatabaseTypeId is required.");
    }
}

internal class DeleteDatabaseType(IDatabaseTypeRepository databaseTypeRepository)
{
    private readonly IDatabaseTypeRepository _databaseTypeRepository = databaseTypeRepository;
    private readonly DeleteDatabaseTypeCommandValidator _validator = new();

    public async Task Handle(DeleteDatabaseTypeCommand command)
    {
        await _validator.ValidateAndThrowAsync(command);
        await _databaseTypeRepository.DeleteDatabaseType(command.DatabaseTypeId);
    }
}
