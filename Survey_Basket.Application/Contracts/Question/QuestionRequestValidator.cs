using FluentValidation;
using Survey_Basket.Domain.Entities;

namespace Survey_Basket.Application.Contracts.Question;

public class QuestionRequestValidator : AbstractValidator<QuestionRequest>
{
    public QuestionRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty()
            .Length(3, 1000);

        RuleFor(x => x.DisplayOrder)
            .GreaterThan(0);

        RuleFor(x => x.Answers).NotNull();

        RuleFor(x => x.Answers)
            .Must((request, answers) =>
            {
                if (request.Type is QuestionType.Number or QuestionType.Text or QuestionType.FileUpload)
                    return answers.Count == 0;

                if (request.Type == QuestionType.TrueFalse)
                    return answers.Count is 0 or 2;

                return answers.Count > 1;
            })
            .WithMessage("Answers list is invalid for selected question type.")
            .When(x => x.Answers != null);

        RuleFor(x => x.Answers)
            .Must(x => x.Distinct().Count() == x.Count)
            .WithMessage("Answers must be unique.")
            .When(x => x.Answers != null);

    }
}
