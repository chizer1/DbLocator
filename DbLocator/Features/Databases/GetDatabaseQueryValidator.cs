using FluentValidation;

namespace DbLocator.Features.Databases;

internal sealed class GetDatabaseQueryValidator : AbstractValidator<GetDatabaseQuery>
{
    internal GetDatabaseQueryValidator()
    {
        RuleFor(x => x.DatabaseId)
            .NotEmpty()
            .WithMessage("Database ID must be provided.")
            .GreaterThan(0)
            .WithMessage("Database ID must be greater than 0.")
            .LessThan(int.MaxValue)
            .WithMessage($"Database ID must be less than {int.MaxValue}.");
    }
}
