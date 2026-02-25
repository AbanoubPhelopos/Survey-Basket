namespace Survey_Basket.Domain.Entities;

public sealed class Company : AuditableEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public string? ContactEmail { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? LogoUrl { get; set; }

    public ICollection<CompanyUser> Users { get; set; } = [];
    public ICollection<PollAudience> TargetedPolls { get; set; } = [];
    public ICollection<Poll> OwnedPolls { get; set; } = [];
    public ICollection<PollOwner> PollOwners { get; set; } = [];
}
