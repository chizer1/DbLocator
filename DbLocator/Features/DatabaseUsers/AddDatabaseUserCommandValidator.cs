using DbLocator.Utilities;
using FluentValidation;

namespace DbLocator.Features.DatabaseUsers;

internal sealed class AddDatabaseUserCommandValidator : AbstractValidator<AddDatabaseUserCommand>
{
    internal AddDatabaseUserCommandValidator()
    {
        RuleFor(x => x.DatabaseIds)
            .NotEmpty()
            .WithMessage("At least one Database ID must be provided.")
            .Must(ids => ids.All(id => id > 0))
            .WithMessage("All Database IDs must be greater than 0.");

        RuleFor(x => x.UserName).MustBeValidName().WithName("Database User Name");

        RuleFor(x => x.UserPassword).MustBeValidPassword().WithName("Database User Password");
    }
}
