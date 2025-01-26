using FluentValidation;

namespace DbLocator.Features.DatabaseServers.UpdateDatabaseServer;

internal record UpdateDatabaseServerCommand(
    int DatabaseServerId,
    string DatabaseServerName,
    string DatabaseServerIpAddress
);

internal sealed class UpdateDatabaseServerCommandValidator
    : AbstractValidator<UpdateDatabaseServerCommand>
{
    public UpdateDatabaseServerCommandValidator()
    {
        RuleFor(x => x.DatabaseServerId).NotNull().WithMessage("Id cannot be null");
        RuleFor(x => x.DatabaseServerName)
            .NotEmpty()
            .WithMessage("DatabaseServer Name is required.");
        RuleFor(x => x.DatabaseServerIpAddress)
            .NotEmpty()
            .WithMessage("Database Server IP Address is required.");
    }
}

internal class UpdateDatabaseServer(IDatabaseServerRepository databaseServerRepository)
{
    private readonly IDatabaseServerRepository _databaseServerRepository = databaseServerRepository;

    public async Task Handle(UpdateDatabaseServerCommand command)
    {
        await new UpdateDatabaseServerCommandValidator().ValidateAndThrowAsync(command);

        await _databaseServerRepository.UpdateDatabaseServer(
            command.DatabaseServerId,
            command.DatabaseServerName,
            command.DatabaseServerIpAddress
        );
    }
}
