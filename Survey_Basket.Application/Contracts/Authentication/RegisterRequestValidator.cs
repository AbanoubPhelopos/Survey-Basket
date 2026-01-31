using FluentValidation;
using Survey_Basket.Application.Abstractions.Const;

namespace Survey_Basket.Application.Contracts.Authentication;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email is required and must be a valid email address.");


        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .Matches(RegexPatterns.PasswordPattern)
            .WithMessage("Password should be at least 8 characters long and contain a mix of letters, numbers, and special characters.");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .WithMessage("First name is required.")
            .Length(3, 100)
            .WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty()
            .WithMessage("Last name is required.")
            .Length(3, 100)
            .WithMessage("Last name must not exceed 100 characters.");

    }
}