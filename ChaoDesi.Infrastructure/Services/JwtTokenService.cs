using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ChaoDesi.Application.Interfaces;
using ChaoDesi.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ChaoDesi.Infrastructure.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user, string userTypeCode)
    {
        var key = _configuration["Jwt:Key"]!;
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];
        var normalizedUserType = string.IsNullOrWhiteSpace(userTypeCode) ? "CUSTOMER" : userTypeCode.Trim().ToUpperInvariant();

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim("customerCode", user.CustomerCode),
            new Claim("userType", normalizedUserType),
            new Claim(ClaimTypes.Role, normalizedUserType)
        };

        if (user.UserTypeId.HasValue)
        {
            claims.Add(new Claim("userTypeId", user.UserTypeId.Value.ToString()));
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
