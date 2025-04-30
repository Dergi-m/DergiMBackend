using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DergiMBackend.Controllers;

/// <summary>
/// Controller for handling authentication-related operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, ISessionService sessionService, IUserService userService) : ControllerBase
{
    private readonly IAuthService authService = authService;
    private readonly ISessionService sessionService = sessionService;
    private readonly IUserService userService = userService;


    /// <summary>
    /// Retrieves an access token using the provided credentials.
    /// </summary>
    /// <param name="request">The token request containing credentials such as client_id, client_secret, and other required fields.</param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains:
    /// - <see cref="OkObjectResult"/> if the token is successfully retrieved, containing the token response.
    /// - <see cref="BadRequestObjectResult"/> if the request parameters are invalid or token retrieval fails.
    /// </returns>
    /// <remarks>
    /// This endpoint expects the request body to be in <c>application/x-www-form-urlencoded</c> format.
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("Token")]
    public async Task<IActionResult> GetAccessToken([FromForm] TokenRequestDto request)
    {
        TokenResponseDto? response = await authService.GetAccessTokenAsync(request);
        return response is null ? Unauthorized("Invalid Credentials") : Ok(response);
    }

    [HttpGet("Session")]
    public async Task<IActionResult> GetCurrentSession()
    {
        var sessionToken = Request.Headers["SessionToken"].FirstOrDefault();

        if (string.IsNullOrEmpty(sessionToken))
        {
            return Unauthorized(new { Success = false, Message = "SessionToken is missing." });
        }

        try
        {
            var principal = await sessionService.ValidateSessionTokenAsync(sessionToken);
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized(new { Success = false, Message = "Invalid session token." });

            var user = await userService.GetUserAsync(userId);

            if (user == null)
                return NotFound(new { Success = false, Message = "User not found." });

            return Ok(new { Success = true, User = user });
        }
        catch (Exception ex)
        {
            return Unauthorized(new { Success = false, Message = ex.Message });
        }
    }

}
