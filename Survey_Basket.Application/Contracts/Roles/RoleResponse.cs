namespace Survey_Basket.Application.Contracts.Roles
{
    public record RoleResponse
    (
        Guid Id,
        string Name,
        bool IsDeleted
    );
}