using System.Security.Claims;

namespace DergiMBackend.Services.IServices;

public interface ISessionService
{
    /// <summary>
    /// Generates a secure session token based on user and organization.
    /// </summary>
    /// <param name="username">The username associated with the session.</param>
    /// <param name="organisation">The organisation the user belongs to.</param>
    /// <returns>JWT-like session token string.</returns>
    string GenerateSessionToken(string username, string organisation);

    /// <summary>
    /// Validates a session token and returns the role claim.
    /// </summary>
    /// <param name="sessionToken">The session token to validate.</param>
    /// <returns>The user's role claim from the token.</returns>
    string ValidateSessionToken(string sessionToken);
}
