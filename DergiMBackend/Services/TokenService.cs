using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DergiMBackend.Models;
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

	public string GenerateToken(string clientId, string clientSecret)
	{
		var key = Encoding.ASCII.GetBytes(clientSecret);
		var tokenHandler = new JwtSecurityTokenHandler();

		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(new Claim[]
			{
			new Claim("clientId", clientId),
			new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(), ClaimValueTypes.Integer64)
			}),
			Expires = DateTime.UtcNow.AddMinutes(5),
			SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
			Issuer = _configuration["ApiSettings:Issuer"],
			Audience = _configuration["ApiSettings:Audience"]
		};

		var token = tokenHandler.CreateToken(tokenDescriptor);
		return tokenHandler.WriteToken(token);
	}

	public ClaimsPrincipal ValidateAccessToken(string token, string clientId)
	{
		ClientConfig client = new()
		{
			ClientId = clientId,
			ClientSecret = _configuration["CLIENT_SECRET"]
		};

		var clientSecret = client.ClientSecret;
		if (string.IsNullOrEmpty(clientSecret))
			throw new Exception("ClientSecret is missing for the given clientId");

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

	public string ValidateSessionToken(string sessionToken)
	{
		if (string.IsNullOrEmpty(sessionToken))
			throw new Exception("SessionToken is required.");

		try
		{
			// Split the token into its parts (Header, Payload, Signature)
			var tokenParts = sessionToken.Split('.');
			if (tokenParts.Length != 3)
				throw new Exception("Invalid SessionToken format.");

			var header = tokenParts[0];
			var payload = tokenParts[1];
			var signature = tokenParts[2];

			// Decode the payload (Base64Url decoding)
			var payloadJson = Encoding.UTF8.GetString(Base64UrlDecode(payload));

			// Parse the payload into a dictionary
			var claims = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(payloadJson);

			// Validate the required claims
			if (!claims.ContainsKey("exp") || !claims.ContainsKey("iss") || !claims.ContainsKey("aud") || !claims.ContainsKey(ClaimTypes.Role))
				throw new Exception("Missing required claims in the SessionToken.");

			// Validate the expiration
			var exp = Convert.ToInt64(claims["exp"]);
			var expirationDate = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
			if (expirationDate < DateTime.UtcNow)
				throw new Exception("SessionToken has expired.");

			// Validate the issuer and audience
			var issuer = claims["iss"].ToString();
			var audience = claims["aud"].ToString();
			if (issuer != _configuration["ApiSettings:Issuer"] || audience != _configuration["ApiSettings:Audience"])
				throw new Exception("Invalid issuer or audience in the SessionToken.");

			// Validate the signature
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["ApiSettings:Secret"]));
			var signingKey = new HMACSHA256(key.Key);
			var computedSignature = Base64UrlEncode(signingKey.ComputeHash(Encoding.UTF8.GetBytes($"{header}.{payload}")));

			if (computedSignature != signature)
				throw new Exception("Invalid SessionToken signature.");

			// Return the role claim
			return claims[ClaimTypes.Role].ToString();
		}
		catch (Exception ex)
		{
			throw new Exception($"An error occurred while validating the SessionToken: {ex.Message}");
		}
	}

	private static byte[] Base64UrlDecode(string input)
	{
		var output = input.Replace('-', '+').Replace('_', '/');
		switch (output.Length % 4)
		{
			case 2: output += "=="; break;
			case 3: output += "="; break;
		}
		return Convert.FromBase64String(output);
	}

	private static string Base64UrlEncode(byte[] input)
	{
		return Convert.ToBase64String(input).Replace('+', '-').Replace('/', '_').TrimEnd('=');
	}
}
