using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Shared.Auth;

public class JwtSettings
{
    [Required] public string Issuer { get; set; } = default!;
    [Required] public string Audience { get; set; } = default!;
    [Required, MinLength(32)] public string SigningKey { get; set; } = default!;
    public int ExpiryMinutes { get; set; } = 120;
}
