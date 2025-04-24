using FluentValidation;

namespace DbLocator.Utilities;

public static class PasswordValidator
{
    public static IRuleBuilderOptions<T, string> MustBeValidPassword<T>(
        this IRuleBuilder<T, string> ruleBuilder
    )
    {
        return ruleBuilder
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters long.")
            .Matches(@"[A-Z]")
            .WithMessage("Password must contain at least one uppercase letter.")
            .Matches(@"[a-z]")
            .WithMessage("Password must contain at least one lowercase letter.")
            .Matches(@"[0-9]")
            .WithMessage("Password must contain at least one number.")
            .Matches(@"[\W_]")
            .WithMessage("Password must contain at least one special character.")
            .MaximumLength(50)
            .WithMessage("Password cannot be more than 50 characters.");
    }
}
