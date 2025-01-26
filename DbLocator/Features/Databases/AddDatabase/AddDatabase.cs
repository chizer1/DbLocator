using DbLocator.Domain;
using FluentValidation;

namespace DbLocator.Features.Databases.AddDatabase;

internal record AddDatabaseCommand(
    string DatabaseName,
    string DatabaseUser,
    int DatabaseServerId,
    byte DatabaseTypeId,
    Status DatabaseStatus,
    bool UseTrustedConnection,
    bool CreateDatabase
);

internal sealed class AddDatabaseCommandValidator : AbstractValidator<AddDatabaseCommand>
{
    public AddDatabaseCommandValidator()
    {
        RuleFor(x => x.DatabaseServerId).GreaterThan(0).WithMessage("Database Server is required.");
        RuleFor(x => x.DatabaseTypeId)
            .GreaterThan((byte)0)
            .WithMessage("Database Type is required.");
        RuleFor(x => x.DatabaseStatus).IsInEnum().WithMessage("Database Status is required.");
    }
}

internal class AddDatabase(IDatabaseRepository databaseRepository)
{
    private readonly IDatabaseRepository _databaseRepository = databaseRepository;

    public async Task<int> Handle(AddDatabaseCommand command)
    {
        await new AddDatabaseCommandValidator().ValidateAndThrowAsync(command);

        return await _databaseRepository.AddDatabase(
            command.DatabaseName,
            command.DatabaseUser,
            command.DatabaseServerId,
            command.DatabaseTypeId,
            command.DatabaseStatus,
            command.UseTrustedConnection,
            command.CreateDatabase
        );
    }
}
