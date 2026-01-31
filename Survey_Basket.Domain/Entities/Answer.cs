namespace Survey_Basket.Domain.Entities;

public sealed class Answer
{
    public Guid Id { get; set; }
    public string Content { get; set; } = string.Empty;

    public Guid QuestionId { get; set; }
    public bool IsActive { get; set; } = true;


    public Question Question { get; set; } = default!;
}
