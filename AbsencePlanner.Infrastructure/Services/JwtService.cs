using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AbsencePlanner.Core.Configuration;
using AbsencePlanner.Core.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AbsencePlanner.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly IOptionsMonitor<JwtSettings> _settings;
    public JwtService(IOptionsMonitor<JwtSettings> settings) => _settings = settings;

    public string GenerateToken(string userId, string role, string name, string email)
    {
        var cfg = _settings.CurrentValue;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(cfg.Secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
            {
            new Claim("userId", userId),
            new Claim(ClaimTypes.Role, role),
            new Claim("name", name),
            new Claim("email", email)
        };
        var token = new JwtSecurityToken(cfg.Issuer, cfg.Audience, claims,
             expires: DateTime.UtcNow.AddMinutes(cfg.ExpirationMinutes), signingCredentials: creds);
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
