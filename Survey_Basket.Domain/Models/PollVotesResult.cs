namespace Survey_Basket.Domain.Entities;

public record PollVotesResult(string Title, IEnumerable<VoteResult> Votes);
public record VoteResult(string VoterName, DateTime SubmittedOn, IEnumerable<QuestionAnswerResult> Answers);
public record QuestionAnswerResult(string Question, string Answer);
