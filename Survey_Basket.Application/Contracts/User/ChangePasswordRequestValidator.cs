using FluentValidation;
using Survey_Basket.Application.Abstractions.Const;

namespace Survey_Basket.Application.Contracts.User;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");


        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .Matches(RegexPatterns.PasswordPattern)
            .WithMessage("Password should be at least 8 characters long and contain a mix of letters, numbers, and special characters.")
            .NotEqual(x => x.CurrentPassword).WithMessage("New password must be different from the current password.");
    }
}
