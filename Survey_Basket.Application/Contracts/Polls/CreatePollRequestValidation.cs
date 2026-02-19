using FluentValidation;

namespace Survey_Basket.Application.Contracts.Polls;

public class CreatePollRequestValidator : AbstractValidator<CreatePollRequests>
{
    public CreatePollRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Summary)
            .NotEmpty()
            .MaximumLength(500);

        RuleFor(x => x.StartedAt)
            .NotEmpty()
            .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today));

        RuleFor(x => x.EndedAt)
            .Must((args, endedAt) => endedAt > args.StartedAt)
            .When(x => x.EndedAt.HasValue)
            .WithMessage("{PropertyName} must be greater than StartedAt");

        RuleFor(x => x.TargetCompanyIds)
            .Must(x => x is null || x.Distinct().Count() == x.Count())
            .WithMessage("TargetCompanyIds must not contain duplicate values.");
    }
}
