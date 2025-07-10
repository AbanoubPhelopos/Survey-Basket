using FluentValidation;

namespace Survey_Basket.Application.Contracts.Polls;

public class CreatePollRequestValidation : AbstractValidator<CreatePollRequests>
{
    public CreatePollRequestValidation()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(500);
    }
}