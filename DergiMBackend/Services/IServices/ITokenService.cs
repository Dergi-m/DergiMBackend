using System.Security.Claims;

namespace DergiMBackend.Services.IServices
{
	public interface ITokenService
	{
		string GenerateToken(string clientId);
		ClaimsPrincipal ValidateToken(string token, string clientId);
	}
}
