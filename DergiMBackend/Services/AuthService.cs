using DergiMBackend.Models;
using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DergiMBackend.Services;
public class AuthService(
    IOptions<Dictionary<string, ClientConfig>> clientsConfig,
    IOptions<ApiSettings> apiSettings,
    IHttpContextAccessor httpContextAccessor,
    ILogger<AuthService> logger) : IAuthService
{
    private readonly Dictionary<string, ClientConfig> clientConfig = clientsConfig.Value;
    private readonly ApiSettings apiSettings = apiSettings.Value;
    private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;
    private readonly ILogger<AuthService> logger = logger;

    public async Task<TokenResponseDto?> GetAccessTokenAsync(TokenRequestDto request)
    {
        ArgumentNullException.ThrowIfNull(request);

        ClientConfig? client = clientConfig.Values.FirstOrDefault(c => c.ClientId == request.ClientId);
        if (client is null)
        {
            LogUnauthorized(request.ClientId);
            return await Task.FromResult<TokenResponseDto?>(null);
        }

        bool verified = VerifyClient(request, client);

        TokenResponseDto? result = await Task.FromResult(verified ? new TokenResponseDto
        {
            AccessToken = GenerateToken(request.ClientId!),
            ExpiresIn = apiSettings.TokenExpiration
        } : null);

        if (result == null)
        {
            LogUnauthorized(request.ClientId);
        }
        else
        {
            logger.LogInformation("Token request succeeded for ClientId {ClientId}.", request.ClientId);
        }

        return result;
    }

    private string GenerateToken(string clientId)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(apiSettings.SecretKey);

        var claims = new List<Claim>
        {
            new Claim("jti", Guid.NewGuid().ToString()),
            new Claim("client_id", clientId),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Audience = apiSettings.Audience,
            Issuer = apiSettings.Issuer,
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.Add(apiSettings.TokenExpiration),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        logger.LogDebug("Generated token for ClientId {ClientId}", clientId);
        return tokenHandler.WriteToken(token);
    }

    private static bool VerifyClient(TokenRequestDto request, ClientConfig client) =>
        request.ClientId == client.ClientId &&
        request.ClientSecret == client.ClientSecret;

    private void LogUnauthorized(string? clientId)
    {
        var httpContext = httpContextAccessor.HttpContext;
        logger.LogWarning("Unauthorized token request from IP {RemoteIpAddress} using ClientId {ClientId}.",
            httpContext?.Connection?.RemoteIpAddress,
            clientId);
    }
}

