namespace Survey_Basket.Application.Contracts.Roles
{
    public record RoleDetailResponse
    (
        Guid Id,
        string Name,
        bool IsDeleted,
        IEnumerable<string> permissions
    );
}