namespace Survey_Basket.Domain.Entities;

public record UserWithRolesResult(Guid Id, string FirstName, string LastName, string Email, bool IsDisabled, IEnumerable<string> Roles);
