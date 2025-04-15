using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DergiMBackend.Services.IServices;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(string clientId)
    {
        var clientSecret = _configuration[$"Clients:{clientId}:ClientSecret"];
        if (string.IsNullOrEmpty(clientSecret))
            throw new Exception("Invalid clientId");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clientSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("clientId", clientId),
            new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["ApiSettings:Issuer"],
            audience: _configuration["ApiSettings:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal ValidateToken(string token, string clientId)
    {
        var clientSecret = _configuration[$"Clients:{clientId}:ClientSecret"];
        if (string.IsNullOrEmpty(clientSecret))
            throw new Exception("Invalid clientId");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(clientSecret));
        var tokenHandler = new JwtSecurityTokenHandler();

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration["ApiSettings:Issuer"],
            ValidAudience = _configuration["ApiSettings:Audience"],
            IssuerSigningKey = key
        };

        return tokenHandler.ValidateToken(token, validationParameters, out _);
    }
}
