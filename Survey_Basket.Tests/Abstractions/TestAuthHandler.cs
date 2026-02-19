using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Survey_Basket.Tests.Abstractions;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private static readonly string TestUserId = "00000000-0000-0000-0000-000000000001";

    public TestAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger, UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "TestUser"),
            new(ClaimTypes.NameIdentifier, TestUserId),
            new("roles", "[\"Admin\",\"Member\"]"),
            // Add all permissions so integration tests can access all endpoints
            new("permissions", "polls:read"),
            new("permissions", "polls:add"),
            new("permissions", "polls:update"),
            new("permissions", "polls:delete"),
            new("permissions", "questions:read"),
            new("permissions", "questions:add"),
            new("permissions", "questions:update"),
            new("permissions", "users:read"),
            new("permissions", "users:add"),
            new("permissions", "users:update"),
            new("permissions", "roles:read"),
            new("permissions", "roles:add"),
            new("permissions", "roles:update"),
            new("permissions", "results:read"),
        };

        var identity = new ClaimsIdentity(claims, "Test");
        identity.AddClaim(new Claim(ClaimTypes.Role, "Admin"));
        identity.AddClaim(new Claim(ClaimTypes.Role, "Member"));

        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
