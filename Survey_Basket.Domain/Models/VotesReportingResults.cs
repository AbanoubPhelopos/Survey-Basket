namespace Survey_Basket.Domain.Entities;

public record VotesPerDayResult(DateOnly Date, int Count);

public record VotesPerQuestionResult(string QuestionContent, IEnumerable<VotesPerAnswerResult> VoteAnswers);
public record VotesPerAnswerResult(string AnswerContent, int Count);
