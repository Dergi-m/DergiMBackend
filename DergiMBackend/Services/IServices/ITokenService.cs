using System.Security.Claims;

namespace DergiMBackend.Services.IServices
{
	public interface ITokenService
	{
		string GenerateToken(string clientId, string clientSecret);
		ClaimsPrincipal ValidateAccessToken(string token, string clientId);
		string ValidateSessionToken(string sessionToken);
	}
}
