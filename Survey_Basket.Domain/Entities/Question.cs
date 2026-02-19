using Survey_Basket.Domain.Models;

namespace Survey_Basket.Domain.Entities;

public sealed class Question : AuditableEntity
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public QuestionType Type { get; set; } = QuestionType.SingleChoice;
    public bool IsRequired { get; set; } = true;
    public int DisplayOrder { get; set; } = 1;

    public Guid PollId { get; set; }
    public bool IsActive { get; set; } = true;


    public Poll Poll { get; set; } = default!;
    public ICollection<Answer> Answers { get; set; } = [];
    public ICollection<VoteAnswers> Votes { get; set; } = [];
}
