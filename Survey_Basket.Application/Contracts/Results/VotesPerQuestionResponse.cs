namespace Survey_Basket.Application.Contracts.Results;

public record VotesPerQuestionResponse(
    string Question,
    IEnumerable<VotesPerAnswerResponse> SelectedAnswers
    );
