using Microsoft.AspNetCore.Identity;

namespace Survey_Basket.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsDisabled { get; set; }

    public List<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<CompanyUser> CompanyUsers { get; set; } = [];
    public ICollection<PollOwner> OwnedPolls { get; set; } = [];

}
