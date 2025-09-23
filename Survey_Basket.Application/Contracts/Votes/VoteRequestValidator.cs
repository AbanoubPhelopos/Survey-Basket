using FluentValidation;

namespace Survey_Basket.Application.Contracts.Votes;

public class VoteRequestValidator : AbstractValidator<VoteRequest>
{
    public VoteRequestValidator()
    {
        RuleFor(x => x.Answers)
            .NotEmpty()
            .WithMessage("At least one answer must be provided.");
    }
}
