namespace Survey_Basket.Domain.Models;

public sealed class Question : AuditableEntity
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;

    public Guid PollId { get; set; }
    public bool IsActive { get; set; } = true;


    public Poll Poll { get; set; } = default!;
    public ICollection<Answer> Answers { get; set; } = [];
    public ICollection<VoteAnswers> Votes { get; set; } = [];
}
