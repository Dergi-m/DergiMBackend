using DergiMBackend.Models.Dtos;
using DergiMBackend.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DergiMBackend.Controllers;

/// <summary>
/// Controller for handling authentication-related operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    private readonly IAuthService authService = authService;

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
}
