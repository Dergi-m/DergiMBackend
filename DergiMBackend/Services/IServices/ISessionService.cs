using DergiMBackend.Models;
using System.Security.Claims;

namespace DergiMBackend.Services.IServices
{
    public interface ISessionService
    {
        /// <summary>
        /// Generates a secure JWT session token for the given user.
        /// </summary>
        /// <param name="user">ApplicationUser entity</param>
        /// <returns>Generated JWT session token as string</returns>
        Task<string> GenerateSessionTokenAsync(ApplicationUser user);

        /// <summary>
        /// Validates an existing JWT session token.
        /// </summary>
        /// <param name="token">JWT session token</param>
        /// <returns>ClaimsPrincipal if valid, or throws if invalid</returns>
        Task<ClaimsPrincipal> ValidateSessionTokenAsync(string sessionToken);

    }
}
