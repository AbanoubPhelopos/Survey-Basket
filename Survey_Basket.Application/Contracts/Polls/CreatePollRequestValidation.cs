using FluentValidation;

namespace Survey_Basket.Application.Contracts.Polls;

public class LoginRequestValidator : AbstractValidator<CreatePollRequests>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);
    }
}