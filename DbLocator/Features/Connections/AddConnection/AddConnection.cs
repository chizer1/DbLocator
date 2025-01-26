using FluentValidation;

namespace DbLocator.Features.Connections.AddConnection;

internal record AddConnectionCommand(int TenantId, int DatabaseId);

internal sealed class AddConnectionCommandValidator : AbstractValidator<AddConnectionCommand>
{
    public AddConnectionCommandValidator()
    {
        RuleFor(command => command.TenantId).NotEmpty().WithMessage("TenantId is required");
        RuleFor(command => command.DatabaseId).NotEmpty().WithMessage("DatabaseId is required");
    }
}

internal class AddConnection(IConnectionRepository connectionRepository)
{
    private readonly IConnectionRepository _connectionRepository = connectionRepository;

    public async Task<int> Handle(AddConnectionCommand command)
    {
        await new AddConnectionCommandValidator().ValidateAndThrowAsync(command);
        return await _connectionRepository.AddConnection(command.TenantId, command.DatabaseId);
    }
}
