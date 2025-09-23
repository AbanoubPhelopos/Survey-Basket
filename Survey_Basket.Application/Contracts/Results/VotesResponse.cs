namespace Survey_Basket.Application.Contracts.Results;

public record VotesResponse(
    string VoterName,
    DateTime VoteDate,
    IEnumerable<QuestionAnswerResponse> SelectedAnswers
    );
