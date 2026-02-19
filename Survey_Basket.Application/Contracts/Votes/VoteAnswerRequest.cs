namespace Survey_Basket.Application.Contracts.Votes;

public record VoteAnswerRequest(
    Guid QuestionId,
    Guid? AnswerId,
    IEnumerable<Guid>? AnswerIds,
    bool? BoolValue,
    decimal? NumberValue,
    string? TextValue,
    FileAnswerRequest? File,
    string? CountryCode
    );
