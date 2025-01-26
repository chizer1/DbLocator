using DbLocator.Domain;
using FluentValidation;

namespace DbLocator.Features.Databases.UpdateDatabase;

internal record UpdateDatabaseCommand(
    int DatabaseId,
    string DatabaseName,
    string DatabaseUsername,
    int DatabaseServerId,
    byte DatabaseTypeId,
    Status DatabaseStatus
);

internal sealed class UpdateDatabaseCommandValidator : AbstractValidator<UpdateDatabaseCommand>
{
    public UpdateDatabaseCommandValidator()
    {
        RuleFor(x => x.DatabaseId).NotNull().WithMessage("Id cannot be null");
        RuleFor(x => x.DatabaseName).NotEmpty().WithMessage("Database Name is required.");
        RuleFor(x => x.DatabaseUsername).NotEmpty().WithMessage("Database Username is required.");
        RuleFor(x => x.DatabaseServerId).NotNull().WithMessage("Database Server is required.");
        RuleFor(x => x.DatabaseTypeId).NotNull().WithMessage("Database Type is required.");
    }
}

internal class UpdateDatabase(IDatabaseRepository databaseRepository)
{
    private readonly IDatabaseRepository _databaseRepository = databaseRepository;

    public async Task Handle(UpdateDatabaseCommand command)
    {
        var validator = new UpdateDatabaseCommandValidator();
        await validator.ValidateAndThrowAsync(command);

        await _databaseRepository.UpdateDatabase(
            command.DatabaseId,
            command.DatabaseName,
            command.DatabaseUsername,
            command.DatabaseServerId,
            command.DatabaseTypeId,
            command.DatabaseStatus
        );
    }
}
