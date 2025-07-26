using FluentValidation;

namespace Survey_Basket.Application.Contracts.Answer;

public class AnswerRequestValidator : AbstractValidator<AnswerRequest>
{
    public AnswerRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .Length(3, 1000);
    }
}
