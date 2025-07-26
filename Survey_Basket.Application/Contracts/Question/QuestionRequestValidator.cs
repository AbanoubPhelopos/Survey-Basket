using FluentValidation;

namespace Survey_Basket.Application.Contracts.Question;

public class QuestionRequestValidator : AbstractValidator<QuestionRequest>
{
    public QuestionRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .Length(3, 1000);

        RuleFor(x => x.Answers).NotNull();

        RuleFor(x => x.Answers)
            .Must(x => x.Count > 1)
            .WithMessage("At least two answers are required.")
            .When(x => x.Answers != null);

        RuleFor(x => x.Answers)
            .Must(x => x.Distinct().Count() == x.Count)
            .WithMessage("Answers must be unique.")
            .When(x => x.Answers != null);

    }
}
