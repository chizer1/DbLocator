using FluentValidation;

namespace DbLocator.Utilities;

public static class UniqueNameValidator
{
    public static IRuleBuilderOptions<T, string> MustBeUnique<T>(
        this IRuleBuilder<T, string> ruleBuilder,
        Func<string, Task<bool>> exists
    )
    {
        return ruleBuilder
            .MustAsync(async (name, _) => !await exists(name))
            .WithMessage("{PropertyName} already exists.");
    }

    public static IRuleBuilderOptions<T, string> MustBeValidName<T>(
        this IRuleBuilder<T, string> ruleBuilder
    )
    {
        return ruleBuilder
            .NotEmpty()
            .WithMessage("{PropertyName} is required.")
            .MaximumLength(50)
            .WithMessage("{PropertyName} cannot be more than 50 characters.")
            .Matches(@"^[a-zA-Z0-9_]+$")
            .WithMessage("{PropertyName} can only contain letters, numbers, and underscores.");
    }
}
