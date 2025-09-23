using System.Security.Claims;

namespace Survey_Basket.Application.Extensions;

public static class UserExtensions
{
    public static Guid GetUserId(this System.Security.Claims.ClaimsPrincipal user) =>
        Guid.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

}
