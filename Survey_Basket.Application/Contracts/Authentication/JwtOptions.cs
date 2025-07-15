using System.ComponentModel.DataAnnotations;

namespace Survey_Basket.Application.Contracts.Authentication;

public class JwtOptions
{
    public static string SectionName => "Jwt";

    [Required]
    public string Key { get; init; } = string.Empty;
    [Required]
    public string Issuer { get; init; } = string.Empty;
    [Required]
    public string Audience { get; init; } = string.Empty;

    [Range(1, int.MaxValue)]
    public int ExpiresIn { get; init; }
}
