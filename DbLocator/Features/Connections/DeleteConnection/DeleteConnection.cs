using FluentValidation;

namespace DbLocator.Features.Connections.DeleteConnection;

internal record DeleteConnectionCommand(int ConnectionId);

internal sealed class DeleteConnectionCommandValidator : AbstractValidator<DeleteConnectionCommand>
{
    public DeleteConnectionCommandValidator()
    {
        RuleFor(command => command.ConnectionId).NotEmpty().WithMessage("ConnectionId is required");
    }
}

internal class DeleteConnection(IConnectionRepository connectionRepository)
{
    private readonly IConnectionRepository _connectionRepository = connectionRepository;

    public async Task Handle(DeleteConnectionCommand command)
    {
        var validator = new DeleteConnectionCommandValidator();
        await validator.ValidateAndThrowAsync(command);

        await _connectionRepository.DeleteConnection(command.ConnectionId);
    }
}
