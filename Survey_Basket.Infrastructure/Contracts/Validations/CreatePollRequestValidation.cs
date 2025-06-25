using FluentValidation;
using Survey_Basket.Infrastructure.Contracts.Requests;

namespace Survey_Basket.Infrastructure.Contracts.Validations;

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