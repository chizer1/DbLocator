using FluentValidation;

namespace DbLocator.Features.DatabaseServers.AddDatabaseServer;

internal record AddDatabaseServerCommand(string DatabaseServerName, string DatabaseServerIpAddress);

internal sealed class AddDatabaseServerCommandValidator
    : AbstractValidator<AddDatabaseServerCommand>
{
    public AddDatabaseServerCommandValidator()
    {
        RuleFor(x => x.DatabaseServerName)
            .NotEmpty()
            .WithMessage("Database Server Name is required.");
        RuleFor(x => x.DatabaseServerIpAddress)
            .NotEmpty()
            .WithMessage("Database Server IP Address is required.");
    }
}

internal class AddDatabaseServer(IDatabaseServerRepository databaseServerRepository)
{
    private readonly IDatabaseServerRepository _databaseServerRepository = databaseServerRepository;

    public async Task<int> Handle(AddDatabaseServerCommand command)
    {
        var validator = new AddDatabaseServerCommandValidator();
        await validator.ValidateAndThrowAsync(command);

        return await _databaseServerRepository.AddDatabaseServer(
            command.DatabaseServerName,
            command.DatabaseServerIpAddress
        );
    }
}
