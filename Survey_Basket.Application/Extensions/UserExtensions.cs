namespace Survey_Basket.Application.Extensions;

public static class UserExtensions
{
    public static Guid GetUserId(this System.Security.Claims.ClaimsPrincipal user)
    {
        var userId = user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userId!);
    }
}
