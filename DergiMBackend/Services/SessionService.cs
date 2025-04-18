using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DergiMBackend.Services.IServices;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

public class SessionService : ISessionService
{
    private readonly IConfiguration _configuration;

    public SessionService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateSessionToken(string username, string organisation)
    {
        if(_configuration == null)
            throw new Exception("Configuration is not set.");

        var issuer = _configuration["ApiSettings:Issuer"] ?? throw new InvalidOperationException("Issuer is not configured.");
        var audience = _configuration["ApiSettings:Audience"] ?? throw new InvalidOperationException("Audience is not configured.");
        var secretKey = _configuration["ApiSettings:SecretKey"] ?? throw new InvalidOperationException("SecretKey is not configured.");


        var header = new { alg = "HS256", typ = "JWT" };
        var encodedHeader = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(header)));

        var now = DateTimeOffset.UtcNow;
        var exp = now.AddMinutes(30).ToUnixTimeSeconds();
        var jti = Guid.NewGuid().ToString();

        var payload = new Dictionary<string, object>
        {
            { "sub", username },
            { "org", organisation },
            { "jti", jti },
            { "iat", now.ToUnixTimeSeconds() },
            { "exp", exp },
            { "iss", issuer },
            { "aud", audience },
            { ClaimTypes.Role, "Member" }
        };

        var encodedPayload = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload)));
        var signingInput = $"{encodedHeader}.{encodedPayload}";

        var signature = ComputeSignature(signingInput, secretKey);
        return $"{signingInput}.{signature}";
    }

    public string ValidateSessionToken(string sessionToken)
    {
        var issuer = _configuration["ApiSettings:Issuer"] ?? throw new InvalidOperationException("Issuer is not configured.");
        var audience = _configuration["ApiSettings:Audience"] ?? throw new InvalidOperationException("Audience is not configured.");
        var secretKey = _configuration["ApiSettings:SecretKey"] ?? throw new InvalidOperationException("SecretKey is not configured.");

        if (string.IsNullOrEmpty(sessionToken))
            throw new Exception("SessionToken is required.");

        var parts = sessionToken.Split('.');
        if (parts.Length != 3)
            throw new Exception("Invalid SessionToken format.");

        var header = parts[0];
        var payload = parts[1];
        var signature = parts[2];

        var claimsJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));
        var claims = JsonSerializer.Deserialize<Dictionary<string, object>>(claimsJson)!;

        if (!claims.ContainsKey("exp") || !claims.ContainsKey("iss") || !claims.ContainsKey("aud") || !claims.ContainsKey(ClaimTypes.Role))
            throw new Exception("Missing required claims in SessionToken.");

        var exp = Convert.ToInt64(claims["exp"]);
        if (DateTimeOffset.FromUnixTimeSeconds(exp) < DateTimeOffset.UtcNow)
            throw new Exception("SessionToken has expired.");

        if (claims["iss"]?.ToString() != issuer
            || claims["aud"]?.ToString() != audience)
            throw new Exception("Invalid issuer or audience.");

        var computedSig = ComputeSignature($"{header}.{payload}", secretKey);
        if (computedSig != signature)
            throw new Exception("Invalid SessionToken signature.");

        return claims[ClaimTypes.Role]?.ToString() ?? "User";
    }

    private static string ComputeSignature(string input, string secret)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        using var algorithm = new HMACSHA256(key.Key);
        var hash = algorithm.ComputeHash(Encoding.UTF8.GetBytes(input));
        return Base64UrlEncode(hash);
    }

    private static byte[] Base64UrlDecode(string input)
    {
        string padded = input.Replace('-', '+').Replace('_', '/');
        switch (padded.Length % 4)
        {
            case 2: padded += "=="; break;
            case 3: padded += "="; break;
        }
        return Convert.FromBase64String(padded);
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input).Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
