using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DergiMBackend.Models;
using DergiMBackend.Services.IServices;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DergiMBackend.Services
{
    public class SessionService : ISessionService
    {
        private readonly ApiSettings _apiSettings;

        public SessionService(IOptions<ApiSettings> apiSettings)
        {
            _apiSettings = apiSettings.Value;
        }

        public async Task<string> GenerateSessionTokenAsync(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? Guid.NewGuid().ToString()), // fallback safe
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty) // safe fallback
            };

            var token = new JwtSecurityToken(
                issuer: _apiSettings.Issuer,
                audience: _apiSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return await Task.FromResult(new JwtSecurityTokenHandler().WriteToken(token));
        }

        public async Task<ClaimsPrincipal> ValidateSessionTokenAsync(string sessionToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_apiSettings.SecretKey));

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _apiSettings.Issuer,
                ValidAudience = _apiSettings.Audience,
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            var principal = tokenHandler.ValidateToken(sessionToken, validationParams, out _);
            return await Task.FromResult(principal);
        }
    }
}
