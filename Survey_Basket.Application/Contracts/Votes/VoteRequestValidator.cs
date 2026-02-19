using FluentValidation;

namespace Survey_Basket.Application.Contracts.Votes;

public class VoteRequestValidator : AbstractValidator<VoteRequest>
{
    public VoteRequestValidator()
    {
        RuleFor(x => x.Answers)
            .NotNull()
            .Must(x => x.Any())
            .WithMessage("At least one answer is required.");

        RuleForEach(x => x.Answers)
            .ChildRules(answer =>
            {
                answer.RuleFor(x => x.QuestionId).NotEmpty();
                answer.RuleFor(x => x.CountryCode)
                    .MaximumLength(3)
                    .When(x => !string.IsNullOrWhiteSpace(x.CountryCode));
            });
    }
}
