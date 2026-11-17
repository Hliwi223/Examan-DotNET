using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace TournoisE_Sports.Services;

public class JwtSettings
{
    public string Issuer { get; set; } = "EsportApi";
    public string Audience { get; set; } = "EsportClient";
    public string Key { get; set; } = string.Empty;
    public int ExpireMinutes { get; set; } = 60;
}

public interface IJwtService
{
    (string token, DateTime expires) CreateToken(IdentityUser user, IList<string> roles);
}

public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;
    public JwtService(IConfiguration config)
    {
        _settings = config.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
        if (string.IsNullOrWhiteSpace(_settings.Key) || _settings.Key.Length < 32)
            throw new InvalidOperationException("Jwt:Key doit contenir au moins 32 caractères (config ou user-secrets).");
    }

    public (string token, DateTime expires) CreateToken(IdentityUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim("sub", user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
        };
        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var expires = DateTime.UtcNow.AddMinutes(_settings.ExpireMinutes);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
