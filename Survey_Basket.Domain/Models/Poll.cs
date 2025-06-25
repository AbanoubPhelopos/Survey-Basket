namespace Survey_Basket.Domain.Models;

public class Poll
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
