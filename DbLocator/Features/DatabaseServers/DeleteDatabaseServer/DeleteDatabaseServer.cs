using FluentValidation;

namespace DbLocator.Features.DatabaseServers.DeleteDatabaseServer;

internal record DeleteDatabaseServerCommand(int DatabaseServerId);

internal sealed class DeleteDatabaseServerCommandValidator
    : AbstractValidator<DeleteDatabaseServerCommand>
{
    public DeleteDatabaseServerCommandValidator()
    {
        RuleFor(x => x.DatabaseServerId).NotEmpty().WithMessage("Id is required.");
    }
}

internal class DeleteDatabaseServer(IDatabaseServerRepository databaseServerRepository)
{
    private readonly IDatabaseServerRepository _databaseServerRepository = databaseServerRepository;

    public async Task Handle(DeleteDatabaseServerCommand command)
    {
        var validator = new DeleteDatabaseServerCommandValidator();
        await validator.ValidateAndThrowAsync(command);

        await _databaseServerRepository.DeleteDatabaseServer(command.DatabaseServerId);
    }
}
