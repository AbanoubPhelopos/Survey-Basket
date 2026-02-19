namespace Survey_Basket.Domain.Entities;

public sealed class PollOwner : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid PollId { get; set; }
    public Guid UserId { get; set; }
    public Guid? CompanyId { get; set; }

    public Poll Poll { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
    public Company? Company { get; set; }
}
