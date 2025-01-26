using FluentValidation;

namespace DbLocator.Features.Databases.DeleteDatabase
{
    internal record DeleteDatabaseCommand(int DatabaseId);

    internal sealed class DeleteDatabaseCommandValidator : AbstractValidator<DeleteDatabaseCommand>
    {
        public DeleteDatabaseCommandValidator()
        {
            RuleFor(x => x.DatabaseId).NotEmpty().WithMessage("Id is required.");
        }
    }

    internal class DeleteDatabase(IDatabaseRepository databaseRepository)
    {
        private readonly IDatabaseRepository _databaseRepository = databaseRepository;

        public async Task Handle(DeleteDatabaseCommand command)
        {
            await new DeleteDatabaseCommandValidator().ValidateAndThrowAsync(command);
            await _databaseRepository.DeleteDatabase(command.DatabaseId);
        }
    }
}
