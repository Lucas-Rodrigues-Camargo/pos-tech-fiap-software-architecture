using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OficinaMecanica.Application.Contracts.Auth;

namespace OficinaMecanica.Api.Auth;

public sealed class JwtTokenService(IConfiguration configuration)
{
    public LoginResponse Generate(string username, IEnumerable<string> roles)
    {
        var expiresAtUtc = DateTime.UtcNow.AddHours(8);
        var key = configuration["Jwt:Key"] ?? "super-secret-key-for-dev-only-2026";
        var issuer = configuration["Jwt:Issuer"] ?? "OficinaMecanica";
        var audience = configuration["Jwt:Audience"] ?? "OficinaMecanica.Admin";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, username),
            new(ClaimTypes.Name, username)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer,
            audience,
            claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new LoginResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
