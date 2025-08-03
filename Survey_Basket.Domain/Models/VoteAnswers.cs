namespace Survey_Basket.Domain.Models;

public sealed class VoteAnswers
{
    public Guid Id { get; set; }
    public Guid VoteId { get; set; }
    public Guid QuestionId { get; set; }
    public Guid AnswerId { get; set; }

    public Vote Vote { get; set; } = default!;
    public Question Question { get; set; } = default!;
    public Answer Answer { get; set; } = default!;

}
