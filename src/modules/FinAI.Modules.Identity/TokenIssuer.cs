using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace FinAI.Modules.Identity;

public sealed record IssuedToken(string AccessToken, DateTime ExpiresAtUtc);

public static class TokenIssuer
{
    private static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(30);

    public static IssuedToken IssueToken(Guid userId, string role, Guid? customerId, IConfiguration cfg)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Role, role)
        };
        if (customerId is { } c && c != Guid.Empty)
            claims.Add(new Claim(ClaimsPrincipalExtensions.CustomerIdClaim, c.ToString()));

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg["JWT:SIGNINGKEY"]!)),
            SecurityAlgorithms.HmacSha256);

        var expires = DateTime.UtcNow.Add(Lifetime);
        var token = new JwtSecurityToken(
            cfg["JWT:ISSUER"],
            cfg["JWT:AUDIENCE"],
            claims,
            expires: expires,
            signingCredentials: creds);
        return new IssuedToken(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
