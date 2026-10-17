using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Ecommerce.Identity.Api.ApplicationLogic;

public interface ITokenIssuer
{
    string Issue(AppUser user, IEnumerable<string> roles);
}

public class TokenIssuer(JwtSettings settings) : ITokenIssuer
{
    public string Issue(AppUser user, IEnumerable<string> roles)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email!),
            new("name", $"{user.FirstName} {user.LastName}".Trim()),
        };
        claims.AddRange(roles.Select(r => new Claim("role", r)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey));
        var token = new JwtSecurityToken(settings.Issuer, settings.Audience, claims,
            expires: DateTime.UtcNow.AddMinutes(settings.ExpiryMinutes),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
