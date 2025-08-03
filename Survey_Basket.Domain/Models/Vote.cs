namespace Survey_Basket.Domain.Models;

public sealed class Vote
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public Guid UserId { get; set; }
    public DateTime SubmittedOn { get; set; } = DateTime.UtcNow;



    public Poll Poll { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
    public ICollection<VoteAnswers> Answers { get; set; } = new List<VoteAnswers>();
}
